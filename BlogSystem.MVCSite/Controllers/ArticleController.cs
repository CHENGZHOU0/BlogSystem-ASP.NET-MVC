using BlogSystem.BLL;
using BlogSystem.MVCSite.Filters;
using BlogSystem.MVCSite.Models.ArticleViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace BlogSystem.MVCSite.Controllers
{
    [BlogSystemAuth]
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        { 
            return View();
        }

        [HttpGet]
        public ActionResult CreateCategory()
        {   
            return View();
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                IBLL.IArticleManager articleManager = new ArticleManager();
                {
                    articleManager.CreateCategory(model.CategoryName, Guid.Parse(Session["userId"].ToString()));
                    return RedirectToAction("CategoryList");
                }
            }
            ModelState.AddModelError("","您输入的信息有误");
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> CategoryList()
        {
            var userId = Guid.Parse(Session["userId"].ToString());
            return View(await new ArticleManager().GetAllCategories(userId));
        }

        [HttpGet]
        public async Task<ActionResult> CreateArticle()
        {
            var userId = Guid.Parse(Session["userId"].ToString());
            ViewBag.CategoryIds = await new ArticleManager().GetAllCategories(userId);
            return View();
        }

        /// <summary>
        /// 创建文章
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]//不做录入校验，直接获取富文本
        public async Task<ActionResult> CreateArticle(Models.ArticleViewModels.CreateArticleViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                var userId = Guid.Parse(Session["userId"].ToString());
                await new ArticleManager().CreateArticle(model.Title, model.Content, model.CategoryIds, userId);
                return RedirectToAction("ArticleList2");
            }

            ModelState.AddModelError("", "添加失败");
            //重定向到CreateArticle，不能直接return view（），return view（）没有ViewBag.CategoryIds
            return RedirectToAction("CreateArticle");
        }

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ArticleList(int pageIndex = 0, int pageSize = 1)
        {
            //给前端 总页码数，当前页码，可显示的总页码数

            var articleMgr = new ArticleManager();

            var userId = Guid.Parse(Session["userId"].ToString());
            var articles = await articleMgr.GetAllArticlesByUserId(userId, pageIndex, pageSize);

            int dataCount = await articleMgr.GetDataCount(userId);

            ViewBag.PageCount = dataCount % pageSize == 0 ? dataCount / pageSize : dataCount / pageSize + 1;
            ViewBag.PageIndex = pageIndex;

            return View(articles);
        }

        /// <summary>
        /// 文章列表2
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ArticleList2(int pageNumber = 1, int pageSize = 3)
        {
            //给前端 总页码数，当前页码，可显示的总页码数
            var articleMgr = new ArticleManager();
            var userId = Guid.Parse(Session["userId"].ToString());
            //当前用户第n页数据
            var articles = await articleMgr.GetAllArticlesByUserId(userId, pageNumber - 1, pageSize);
            //当前用户文章总数
            int dataCount = await articleMgr.GetDataCount(userId);
            //调用MvcPager的 PagedList
            return View(new PagedList<Dto.ArticleDto>(articles, pageNumber, pageSize, dataCount));
        }

        /// <summary>
        /// 文章详情页
        /// </summary>
        /// <param name="id">ArticleId,?表示可以为空</param>
        /// <returns></returns>
        public async Task<ActionResult> ArticleDetails(Guid? id)
        {
            var articleMgr = new ArticleManager();
            if (id == null || !await articleMgr.ExistArticle(id.Value))
            {
                return RedirectToAction(nameof(ArticleList));
            }

            ViewBag.Comments = await articleMgr.GetCommentsByArticleId(id.Value);

            return View(await articleMgr.GetOneArticleById(id.Value));
        }

        /// <summary>
        /// 编辑文章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> EditArticle(Guid id)
        {
            //需要对id进行校验！！！

            IBLL.IArticleManager articleManager = new ArticleManager();
            var data = await articleManager.GetOneArticleById(id);

            var userId = Guid.Parse(Session["userId"].ToString());
            ViewBag.CategoryIds = await new ArticleManager().GetAllCategories(userId);

            return View(new Models.ArticleViewModels.EditArticleViewModel()
            {
                Title = data.Title,
                Content = data.Content,
                CategoryIds = data.CategoryIds,
                Id = data.Id
            });
        }

        /// <summary>
        /// 编辑文章
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> EditArticle(Models.ArticleViewModels.EditArticleViewModel model) 
        {
            if (ModelState.IsValid)
            {
                IBLL.IArticleManager articleManager = new ArticleManager();
                await articleManager.EditArticle(model.Id, model.Title, model.Content, model.CategoryIds);
                return RedirectToAction("ArticleList2");
            }
            else
            {
                var userId = Guid.Parse(Session["userId"].ToString());
                ViewBag.CategoryIds = await new ArticleManager().GetAllCategories(userId);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddGoodCount(Guid id)
        {
            IBLL.IArticleManager articleManager = new ArticleManager();
            await articleManager.AddGoodCount(id);
            return Json(new { result = "ok"});
        }

        [HttpPost]
        public async Task<ActionResult> AddBadCount(Guid id)
        {
            IBLL.IArticleManager articleManager = new ArticleManager();
            await articleManager.AddBadCount(id);
            return Json(new { result = "ok" });
        }

        [HttpPost]
        public async Task<ActionResult> AddComment(Models.ArticleViewModels.CreateCommentViewModel model)
        {
            var useId = Guid.Parse(Session["useId"].ToString());
            IBLL.IArticleManager articleManager = new ArticleManager();
            await articleManager.CreateComment(useId, model.Id, model.Content);
            return Json(new { result = "ok" });
        }
    }
}