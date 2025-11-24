using System.Net.Http.Headers;

string baseUrl = Environment.GetEnvironmentVariable("SIM_TARGET_URL") ?? "http://localhost:8080"; // default local API port
const int intervalSeconds = 10;
const int count = 0;
const double amplitudeC = 5;
const double periodSeconds = 600;
const double baselineC = 20.0;
const double phaseDeg = 0.0; // phase shift

DateTime startUtc = DateTime.UtcNow;

Console.WriteLine($"Ecowitt simulator starting. Target: {baseUrl}, interval: {intervalSeconds}s, count: {count}");
Console.WriteLine($"Wave config: amplitudeC={amplitudeC}, periodSeconds={periodSeconds}, baselineC={baselineC}, phaseDeg={phaseDeg}");

try
{
    using CancellationTokenSource cts = new();
    Console.CancelKeyPress += (s, e) =>
    {
        Console.WriteLine("\nCancellation requested... shutting down.");
        e.Cancel = true; // prevent immediate process kill
        cts.Cancel();
    };

    using HttpClient client = new();
    client.Timeout = TimeSpan.FromSeconds(10);

    string url = baseUrl.TrimEnd('/') + "/api/webhook";
    Console.WriteLine($"POST target {url}. Press Ctrl+C to stop.");

    if (count > 0)
    {
        for (int n = 0; n < count; n++)
        {
            Dictionary<string, string> form = BuildSamplePayload();
            using FormUrlEncodedContent content = new(form);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            Console.WriteLine($"POST {url} -> sending {form.Count} fields");
            HttpResponseMessage resp = await client.PostAsync(url, content, cts.Token);
            string body = await resp.Content.ReadAsStringAsync(cts.Token);
            Console.WriteLine($"Response {(int)resp.StatusCode} {resp.ReasonPhrase}: {body}");

            if (n < count - 1)
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cts.Token);
        }
    }
    else
    {
        int n = 0;
        while (!cts.IsCancellationRequested)
        {
            Dictionary<string, string> form = BuildSamplePayload();
            using FormUrlEncodedContent content = new(form);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            Console.WriteLine($"POST {url} (iteration {++n}) -> sending {form.Count} fields");
            HttpResponseMessage resp = await client.PostAsync(url, content, cts.Token);
            string body = await resp.Content.ReadAsStringAsync(cts.Token);
            Console.WriteLine($"Response {(int)resp.StatusCode} {resp.ReasonPhrase}: {body}");

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cts.Token);
        }
    }

    Console.WriteLine("Simulation finished.");
    return 0;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Simulation canceled.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Simulator error: {ex}");
    return 1;
}

Dictionary<string, string> BuildSamplePayload()
{
    DateTime nowUtc = DateTime.UtcNow;

    double seconds = (nowUtc - startUtc).TotalSeconds;
    double phaseRad = phaseDeg * Math.PI / 180.0;
    double tempC = baselineC + amplitudeC * Math.Sin(2 * Math.PI * seconds / periodSeconds + phaseRad);
    double tempF = tempC * 9.0 / 5.0 + 32.0;

    // Only dateutc and tempf
    Dictionary<string, string> payload = new()
    {
        ["dateutc"] = nowUtc.ToString("yyyy-MM-dd HH:mm:ss"),
        ["tempf"] = tempF.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
    };

    return payload;
}