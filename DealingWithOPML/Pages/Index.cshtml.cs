using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;


namespace DealingWithOPML.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public List<RssFeed> RssFeedList { get; private set; } = new();
        public int CurrentPage { get; private set; }

        public async Task<List<RssFeed>> FetchParse()
        {
            List<RssFeed> feedList = new List<RssFeed>();
            HttpClient httpClient = _clientFactory.CreateClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");
            if (httpResponse.IsSuccessStatusCode)
            {
                HttpContent responseContent = httpResponse.Content;
                string responseData = await responseContent.ReadAsStringAsync();
                Console.WriteLine(responseData);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseData);
                XmlNodeList? nodes = doc.DocumentElement?.GetElementsByTagName("outline");

                foreach (XmlNode node in nodes)
                {
                    RssFeed feedObject = new RssFeed();
                    feedObject.FeedTitle = node.Attributes?["text"]?.Value;
                    feedObject.FeedLink = node.Attributes?["xmlUrl"]?.Value;
                    feedList.Add(feedObject);
                }
            }
            httpClient.Dispose();
            return feedList;
        }
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int itemsPerPage = 5)
        {
            var myFeedList = await FetchParse();
            int startIndex = (pageNumber - 1) * itemsPerPage;
            int totalItems = myFeedList.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            if (startIndex >= totalItems)
            {
                startIndex = (totalPages - 1) * itemsPerPage;
                pageNumber = totalPages;
            }
            RssFeedList = myFeedList.GetRange(startIndex, Math.Min(itemsPerPage, totalItems - startIndex));
            CurrentPage = pageNumber;
            ViewData["TotalPages"] = totalPages;
            return Page();
        }
    }
    public class RssFeed
    {
        public string? FeedTitle { get; set; }
        public string? FeedLink { get; set; }
    }



}
