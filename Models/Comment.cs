using System;

namespace BestStore.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; } = "";
        public string Emoji { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Product? Product { get; set; }
    }
}
