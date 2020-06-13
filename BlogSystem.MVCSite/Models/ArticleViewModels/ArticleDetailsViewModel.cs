using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogSystem.MVCSite.Models.ArticleViewModels
{
    /// <summary>
    /// 如果该ViewModel和Dto中的类一样可以省略，可以使用Dto中的类
    /// 如果不同，必须创建
    /// </summary>
    public class ArticleDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public DateTime CreateTime { get; set; }

        public string Content { get; set; }

        public string Email { get; set; }

        public string ImagePath { get; set; }

        public Guid[] CategoryIds { get; set; }

        public string[] CategoryNames { get; set; }

        public int GoodCount { get; set; }

        public int BadCount { get; set; }
    }
}