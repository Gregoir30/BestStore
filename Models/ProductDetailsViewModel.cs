using System.Collections.Generic;

namespace BestStore.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = new Product();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int LikeCount { get; set; }
        public bool UserHasLiked { get; set; }
    }
}
