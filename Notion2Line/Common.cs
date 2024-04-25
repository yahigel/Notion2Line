using Newtonsoft.Json.Linq;

namespace Notion2Line
{
    static class Common
    {
        public static List<StudyThema> FilterNotionResultsByDate(string jsonResponse)
        {
            var filteredResults = new List<StudyThema>();
            var data = JObject.Parse(jsonResponse);
            var results = data["results"] as JArray;

            DateTime now = DateTime.Now;
            DateTime weekLater = now.AddDays(7);

            foreach (var item in results)
            {
                // NullReferenceExceptionを防ぐために、安全なアクセスを使用
                JToken dateToken = item["properties"]?["研究定例発表日"]?["date"];
                string startDateString = dateToken?["start"]?.ToString();

                if (!string.IsNullOrEmpty(startDateString) && DateTime.TryParse(startDateString, out DateTime startDate))
                {
                    if (startDate > now && startDate < weekLater)
                    {
                        // Null安全なプロパティアクセス
                        string researchThemaName = (item["properties"]?["研究定例発表"]?["title"]?[0]?["text"]?["content"] as JToken)?.ToString() ?? "";
                        string presenterName = (item["properties"]?["実施者"]?["select"]?["name"] as JToken)?.ToString() ?? "";

                        var studyThema = new StudyThema
                        {
                            ResearchThemaName = researchThemaName,
                            PresentationDate = startDate,
                            PresenterName = presenterName
                        };
                        filteredResults.Add(studyThema);
                    }
                }
            }

            return filteredResults;
        }
    }
}
