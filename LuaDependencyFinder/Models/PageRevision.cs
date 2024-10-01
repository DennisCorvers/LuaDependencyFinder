namespace LuaDependencyFinder.Models
{
    public record class PageRevision
    {
        public int PageId { get; }

        public int Namespace { get; }

        public string Title { get; }

        public DateTime? LatestRevision { get; }

        public PageRevision(int pageId, int @namespace, string title, DateTime? latestRevision)
        {
            PageId = pageId;
            Namespace = @namespace;
            Title = title;
            LatestRevision = latestRevision;
        }
    }
}
