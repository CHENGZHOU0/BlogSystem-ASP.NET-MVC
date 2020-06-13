using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.IBLL
{
    public interface IArticleManager
    {
        Task CreateArticle(string title, string context, Guid[] categoryIds, Guid userId);

        Task CreateCategory(string name, Guid userId);

        Task<List<Dto.BlogCategoryDto>> GetAllCategories(Guid userId);

        Task<List<Dto.ArticleDto>> GetAllArticlesByUserId(Guid userId, int pageIndex, int pageSize);

        /// <summary>
        /// 获取数据总数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<int> GetDataCount(Guid userId);

        Task<List<Dto.ArticleDto>> GetAllArticlesByEmail(string email);

        Task<List<Dto.ArticleDto>> GetAllArticlesByCategoryId(Guid categoryId);

        Task RemoveCategory(string name);

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <param name="newCategoryName"></param>
        /// <returns></returns>
        Task EditCategory(Guid CategoryId, string newCategoryName);

        Task RemoveArticle(Guid articleId);

        /// <summary>
        /// 编辑文章
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="categoryIds"></param>
        /// <returns></returns>
        Task EditArticle(Guid articleId, string title, string content, Guid[] categoryIds);

        Task<bool> ExistArticle(Guid articleId);

        Task<Dto.ArticleDto> GetOneArticleById(Guid articleId);

        Task AddGoodCount(Guid articleId);

        Task AddBadCount(Guid articleId);

        Task CreateComment(Guid userId, Guid articleId, string content);

        Task<List<Dto.CommentDto>> GetCommentsByArticleId(Guid articleId);
    }
}
