namespace Frontend;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Extensions.Configuration;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .UseSentry(options => {
                // The DSN is the only required setting.
                options.Dsn = "https://5248e12f92b54298a28ce1aa02a1ff62@o86280.ingest.sentry.io/4505081436766208";
            });

#if ANDROID || IOS
        builder.UseShiny();
        builder.Configuration.AddJsonPlatformBundle(optional: false);

        var cfg = builder.Configuration.GetSection("Firebase");
        builder.Services.AddPushFirebaseMessaging(new(
            false,
            cfg["AppId"],
            cfg["SenderId"],
            cfg["ProjectId"],
            cfg["ApiKey"]
        ));
#endif
        return builder.Build();
    }
}
