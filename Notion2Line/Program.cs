using Microsoft.Extensions.Configuration;
using Notion2Line;
using System.Globalization;

IConfiguration config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                           .Build();

// コンフィグレーション値の読み取り
string notionSecret = config["Notion:Secret"];
string databaseId = config["Notion:DatabaseId"];
string lineToken = config["Line:Token"];

Console.WriteLine("Start Notion to Line");

// 投稿するメッセージ
string message = new String("");

//ノーションからデータ取得してメッセージ構築
var notionClient = new NotionApiClient(notionSecret);
try
{
    var notionData = await notionClient.FetchAllDataFromNotion(databaseId);
    List<StudyThema> resList = Common.FilterNotionResultsByDate(notionData.ToString());



    if (resList.Count > 0)
    {
        CultureInfo japaneseCulture = CultureInfo.GetCultureInfo("ja-JP");
        message += resList[0].PresentationDate.ToString("\nMM/dd (ddd)", japaneseCulture) + "の研究定例の発表テーマは...\n\n";

        foreach (StudyThema item in resList)
        {
            message += item.PresenterName + "さん「" + item.ResearchThemaName + "」\n";
        }
        message += "\nお楽しみに！";
        Console.WriteLine(message);
    }
    else
    {
        message += "\n直近の研究定例で発表する研究テーマが決まっているか確認してください！";
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

//ここからはLINE
var lineClient = new LineNotifyClient(lineToken);

try
{
    await lineClient.NotifyAsync(message);
}
catch (Exception ex)
{
    Console.WriteLine($"Error sending LINE notification: {ex.Message}");
}
finally
{
    lineClient.Dispose();
}

