using System;
using System.Collections.Generic;
using HarmonyLib;

[HarmonyPatch(typeof(CraftComponent), "IsAutoCraftable", MethodType.Getter)]
public static class ForceAutoCraftPatch
{
    // --- НАШ БЕЛЫЙ СПИСОК ВЕРСТАКОВ ---
    private static readonly HashSet<string> AllowedAutoCrafterIds = new HashSet<string>
    {
        "saw_bench",
        "furnace_1",
        "furnace_2"
    };

    public static bool Prefix(CraftComponent __instance, ref bool __result)
    {
        if (__instance == null || __instance.CraftableObject == null) return true;

        WgoData wgoData = null;

        try
        {
            // ПРАВИЛЬНЫЙ И БЕЗОПАСНЫЙ ПУТЬ БЕЗ РЕФЛЕКСИИ ПО КЛАССУ WgoData:
            // Приводим CraftableObject к его реальному типу в рантайме.
            // В игре все производственные объекты унаследованы от WgoData или содержат его.
            if (__instance.CraftableObject is WgoData directData)
            {
                wgoData = directData;
            }
            else
            {
                // Если объект крафта обернут в другую сущность, безопасно достаем WgoData 
                // через интерфейс ICraftable. Проверяем свойство WgoData конкретно у CraftableObject
                var prop = AccessTools.Property(__instance.CraftableObject.GetType(), "WgoData")
                           ?? AccessTools.Property(__instance.CraftableObject.GetType(), "wgoData")
                           ?? AccessTools.Property(__instance.CraftableObject.GetType(), "Data");

                if (prop != null)
                {
                    wgoData = prop.GetValue(__instance.CraftableObject) as WgoData;
                }
            }
        }
        catch (Exception)
        {
            // Игнорируем молча, чтобы не спамить в логи при технических проверках
        }

        // Если не смогли определить верстак — отдаем управление ванили
        if (wgoData == null) return true;

        // Проверяем белый список
        bool isAllowed = AllowedAutoCrafterIds.Contains(wgoData.id) ||
                         (wgoData.Definition != null && AllowedAutoCrafterIds.Contains(wgoData.Definition.wgoGroup));

        if (isAllowed)
        {
            var current = __instance.CurrentCraftElement;
            if (current == null || current.Def == null) return true;

            // Запрещаем автокрафт для заправки топливом
            if (current.Def.isFuelCraft)
            {
                return true;
            }

            // Включаем автоматику!
            __result = true;
            return false; // Блокируем оригинал
        }

        return true;
    }
}