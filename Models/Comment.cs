using System;

namespace DDAC_Assignment.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public int NewsId { get; set; }
        public string CommentBy { get; set; }
        public DateTime CommentDateTime { get; set; }

    }
}