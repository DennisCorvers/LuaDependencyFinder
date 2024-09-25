namespace LuaDependencyFinder.Models
{
    public record class PageInfo(int PageId, int Namespace, string Title, DateTime? LatestRevision)
    {
    }
}
