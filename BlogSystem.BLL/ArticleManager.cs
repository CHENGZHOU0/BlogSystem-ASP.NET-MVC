using BlogSystem.DAL;
using BlogSystem.Dto;
using BlogSystem.IBLL;
using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.BLL
{
    public class ArticleManager : IArticleManager
    {
        /// <summary>
        /// 新建文章
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="categoryIds"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task CreateArticle(string title, string content, Guid[] categoryIds, Guid userId)
        {
            using (var articleSvc = new ArticleService())
            {
                var article = new Article()
                {
                    Title = title,
                    Content = content,
                    UserId = userId
                };
                await articleSvc.CreateAsync(article);

                Guid articleId = article.Id;

                using (var articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var categoryId in categoryIds)
                    {
                        await articleToCategorySvc.CreateAsync(new ArticleToCategory()
                        {
                            ArticleId = articleId,
                            BlogCategoryId = categoryId
                        }, saved: false);
                    }

                    await articleToCategorySvc.Save();
                }
            }
        }

        /// <summary>
        /// 新建分类
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task CreateCategory(string name, Guid userId)
        {
            using (var categorySvs = new BlogCategoryService())
            {
                await categorySvs.CreateAsync(new BlogCategory()
                {
                    CategoryName = name,
                    UserId = userId
                });
            }
        }

        /// <summary>
        /// 编辑文章
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="categoryIds"></param>
        /// <returns></returns>
        public async Task EditArticle(Guid articleId, string title, string content, Guid[] categoryIds)
        {
            using (IDAL.IArticleService articleSvc = new ArticleService())
            {
                var article = await articleSvc.GetOneByIdAsync(articleId);
                article.Title = title;
                article.Content = content;
                await articleSvc.EditAsync(article);

                using (IDAL.IArticleToCategoryService articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var categoryId in articleToCategorySvc.GetAllAsync().Where(m => m.ArticleId == articleId))
                    {
                        await articleToCategorySvc.RemoveAsync(categoryId, saved: false);
                    }
                    foreach (var categoryId in categoryIds)
                    {
                        await articleToCategorySvc.CreateAsync(new ArticleToCategory() { ArticleId = articleId, BlogCategoryId = categoryId }, saved: false);
                    }
                    await articleToCategorySvc.Save();
                }
            }
        }

        public Task EditCategory(Guid CategoryId, string newCategoryName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据Id判断文章是否存在
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task<bool> ExistArticle(Guid articleId)
        {
            using (IDAL.IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAllAsync().AnyAsync(m => m.Id == articleId);
            } 
        }

        public Task<List<ArticleDto>> GetAllArticlesByCategoryId(Guid categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ArticleDto>> GetAllArticlesByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据UserId得到第n页文章
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<List<ArticleDto>> GetAllArticlesByUserId(Guid userId, int pageIndex, int pageSize)
        {
            using (IDAL.IArticleService articleSvc = new ArticleService())
            {
                var list = await articleSvc.GetAllByPageOrderAsync(pageSize, pageIndex, false).Include(m => m.User).Where(m => m.UserId == userId).Select(m => new ArticleDto()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    CreateTime = m.CreateTime,
                    Email = m.User.Email,
                    GoodCount = m.GoodCount,
                    BadCount = m.BadCount,
                    ImagePath = m.User.ImagePath

                }).ToListAsync();

                using (IDAL.IArticleToCategoryService articleToCategorySvc = new ArticleToCategoryService())
                {
                    foreach (var articleDto in list)
                    {
                        var cates = await articleToCategorySvc.GetAllAsync().Include(m => m.BlogCategory).Where(m => m.ArticleId == articleDto.Id).ToListAsync();
                        articleDto.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                        articleDto.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// 根据UserId的到文章总数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> GetDataCount(Guid userId)
        {
            using (IDAL.IArticleService articleSvc = new ArticleService())
            {
                return await articleSvc.GetAllAsync().CountAsync(m => m.UserId == userId);
            }
        }

        /// <summary>
        /// 得到所有的分类
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<BlogCategoryDto>> GetAllCategories(Guid userId)
        {
            using (IDAL.IBlogCategory categorySvc = new BlogCategoryService())
            {
                return await categorySvc.GetAllAsync().Where(m => m.UserId == userId).Select(m => new Dto.BlogCategoryDto()
                {
                    Id = m.Id,
                    CategoryName = m.CategoryName,
                }).ToListAsync();
            }
        }

        /// <summary>
        /// 通过文章Id获取文章
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task<ArticleDto> GetOneArticleById(Guid articleId)
        {
            using (IDAL.IArticleService articleSvc = new ArticleService())
            {
                 var data =  await articleSvc.GetAllAsync().Include(m => m.User).Where(m => m.Id == articleId).Select(m => new Dto.ArticleDto()
                {
                    Id = m.Id,
                    BadCount = m.BadCount,
                    GoodCount = m.GoodCount,
                    Title = m.Title,
                    Content = m.Content,
                    CreateTime = m.CreateTime,
                    ImagePath = m.User.ImagePath,
                    Email = m.User.Email
                }).FirstAsync();

                using (IDAL.IArticleToCategoryService articleToCategorySvc = new ArticleToCategoryService())
                {
                    var cates = await articleToCategorySvc.GetAllAsync().Include(m => m.BlogCategory).Where(m => m.ArticleId == data.Id).ToListAsync();
                    data.CategoryIds = cates.Select(m => m.BlogCategoryId).ToArray();
                    data.CategoryNames = cates.Select(m => m.BlogCategory.CategoryName).ToArray();
                    return data;
                }

            }
        }

        public Task RemoveArticle(Guid articleId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveCategory(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 顶
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task AddGoodCount(Guid articleId)
        {
            using (IDAL.IArticleService articleService = new ArticleService())
            {
                var article = await articleService.GetOneByIdAsync(articleId);
                article.GoodCount++;
                await articleService.EditAsync(article);
            }
        }

        /// <summary>
        /// 踩
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        public async Task AddBadCount(Guid articleId)
        {
            using (IDAL.IArticleService articleService = new ArticleService())
            {
                var article = await articleService.GetOneByIdAsync(articleId);
                article.BadCount++;
                await articleService.EditAsync(article);
            }
        }

        public async Task CreateComment(Guid userId, Guid articleId, string content)
        {
            using (IDAL.ICommentService commentService = new CommentService())
            {
                await commentService.CreateAsync(new Comment()
                {
                    UserId = userId,
                    ArticleId = articleId,
                    Content = content
                });
            }
        }

        public async Task<List<Dto.CommentDto>> GetCommentsByArticleId(Guid articleId)
        {
            using (IDAL.ICommentService commentService = new CommentService())
            {
                return await commentService.GetAllAsync().Where(m => m.ArticleId == articleId).Include(m => m.User).Select(m => new Dto.CommentDto()
                {
                    Id = m.Id,
                    ArticleId = m.ArticleId,
                    UserId = m.UserId,
                    Email = m.User.Email,
                    Content = m.Content,
                    CreateTime = m.CreateTime
                }).ToListAsync();
            };
        }


    }
}
