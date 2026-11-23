using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace DescFix.Server;

/// <summary>
/// Overrides individual entries of the Chinese ("ch") global locale with the corrected texts in
/// db/ch.json. Only text is touched: no item, quest, trader or hideout data is read or written.
///
/// The override is registered as a lazy-load transformer, so it is re-applied every time the locale
/// is (re)built and runs after the base file has been deserialised. It is registered late
/// (<see cref="OnLoadOrder.PostLoad"/>) so that locale entries added by other mods are already in
/// place; keys that no longer exist in the base locale are still written, which is harmless.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad)]
public class DescFixLoader(
    ISptLogger<DescFixLoader> logger,
    ModHelper modHelper,
    LocaleTable localeTable) : IOnLoad
{
    private const string Language = "ch";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        string file = Path.Combine(modPath, "db", "ch.json");

        Dictionary<string, string>? overrides;
        try
        {
            using FileStream stream = File.OpenRead(file);
            overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
        }
        catch (Exception ex)
        {
            logger.Error($"DescFix: cannot read {file}: {ex.Message}");
            return Task.CompletedTask;
        }

        if (overrides == null || overrides.Count == 0)
        {
            logger.Warning("DescFix: db/ch.json is empty, nothing to apply");
            return Task.CompletedTask;
        }

        if (!localeTable.Global.TryGetValue(Language, out var lazyLocale))
        {
            logger.Warning($"DescFix: locale '{Language}' not found, nothing applied");
            return Task.CompletedTask;
        }

        lazyLocale.AddTransformer(locale =>
        {
            if (locale == null)
            {
                return locale;
            }

            foreach (var (key, text) in overrides)
            {
                locale[key] = text;
            }
            return locale;
        });

        logger.Success($"DescFix: {overrides.Count} Chinese descriptions applied");
        return Task.CompletedTask;
    }
}
