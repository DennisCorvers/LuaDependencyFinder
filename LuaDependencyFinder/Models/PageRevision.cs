namespace LuaDependencyFinder.Models
{
    public record PageRevision(int PageId, int Namespace, string Title, DateTime? LatestRevision);
}
