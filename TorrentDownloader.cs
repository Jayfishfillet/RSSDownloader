using System.Collections.Specialized;
using System.Net;
using System.Xml.Linq;

Console.Write("Enter RSS feed link: ");
string rssLink = Console.ReadLine();

Console.Write(@"Enter Parent directory: ");
string parentDirectory = Console.ReadLine();

Console.Write("Enter folder name: ");
string folderName = Console.ReadLine();

Console.WriteLine("");

string fullPath = Path.Combine(parentDirectory, folderName);

try
{
    string rss = DownloadRssFeedAsync(rssLink).Result;

    ParseFeed(rss, fullPath, folderName);

    Console.WriteLine("Torrents added to download queue successfully!");
    Console.WriteLine("Press any key to exit.");
    Console.ReadKey(true);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

static async Task<string> DownloadRssFeedAsync(string rssLink)
{
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync(rssLink);
        response.EnsureSuccessStatusCode();
        string rss = await response.Content.ReadAsStringAsync();
        return rss;
    }
}

static void ParseFeed(string rss, string fullPath, string folderName)
{
    XDocument doc = XDocument.Parse(rss);
    foreach (XElement item in doc.Descendants("item"))
    {
        string showTitle = item.Element("title").Value;
        string showLink = item.Element("link").Value;

        using (var client = new WebClient())
        {
            var values = new NameValueCollection
            {
                { "urls", showLink },
                { "category", folderName },
                { "savepath", fullPath }
            };
            client.UploadValues("http://localhost:8080/api/v2/torrents/add", values);
        }
    }
}
