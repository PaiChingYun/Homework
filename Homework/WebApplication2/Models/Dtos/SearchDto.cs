namespace WebApplication2.Models.Dtos
{
    public class SearchDto
    {
        //搜尋相關
        public string? keyword { get; set; }
        public int? categoryId { get; set; } = 0; //預設全部，不根據類別搜尋

        //排序相關
        public string? sortBy { get; set; }
        public string? sortType { get; set; } = "asc";//預設升冪排序

        //分頁相關
        public int? page { get; set; } = 1;//第一頁開始
        public int? pageSize {  get; set; } = 10;//預設每頁10筆
    }
}
