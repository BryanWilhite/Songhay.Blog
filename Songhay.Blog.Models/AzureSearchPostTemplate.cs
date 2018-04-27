namespace Songhay.Blog.Models
{
    public class AzureSearchPostTemplate
    {
        public bool Count { get; set; }
        public int MinimumCoverage { get; set; }
        public string QueryType { get; set; }
        public string Search { get; set; }
        public string SearchFields { get; set; }
        public string SearchMode { get; set; }
        public string Select { get; set; }
        public int Skip { get; set; }
        public int Top { get; set; }
    }
}
