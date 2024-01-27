namespace WebApplication2.Models.Dtos
{
    public class SpotsPagingDto
    {
        public int TotalPages { get; set; }
        public List<SpotImagesSpot>? SpotsResult { get; set; }
    }
}
