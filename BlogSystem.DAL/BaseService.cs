using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.DAL
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity,new()
    {

        //上下文
        private readonly BlogContext _db;
        public BaseService(Models.BlogContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(T model, bool saved = true)
        {
            _db.Set<T>().Add(model);
            if (saved) await _db.SaveChangesAsync();
        }

        /// <summary>
        /// IDisposable接口
        /// </summary>
        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saved"></param>
        /// <returns></returns>
        public async Task EditAsync(T model, bool saved = true)
        {
            _db.Configuration.ValidateOnSaveEnabled = false;
            _db.Entry(model).State = EntityState.Modified;
            if (saved)
            {
                await _db.SaveChangesAsync();
                _db.Configuration.ValidateOnSaveEnabled = true;
            }
                
        }

        /// <summary>
        /// 返回所有未被删除的数据（没有真的执行）
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetAllAsync()
        {
            return _db.Set<T>().Where(m => !m.IsRemoved).AsNoTracking();
        }

        /// <summary>
        /// 按页取得所有数据
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IQueryable<T> GetAllByPageAsync(int pageSize = 10, int pageIndex = 0)
        {
            return GetAllAsync().Skip(pageSize * pageIndex).Take(pageSize);
        }

        /// <summary>
        /// 按页得到所有数据并排序
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public IQueryable<T> GetAllByPageOrderAsync(int pageSize = 10, int pageIndex = 0, bool asc = true)
        {
            return GetAllOrderAsync().Skip(pageSize * pageIndex).Take(pageSize);
        }

        /// <summary>
        /// 得到所有数据并排序
        /// </summary>
        /// <param name="asc"></param>
        /// <returns></returns>
        public IQueryable<T> GetAllOrderAsync(bool asc = true)
        {
            var datas = GetAllAsync();
            if (asc)
            {
                datas = datas.OrderBy(m => m.CreateTime);
            }
            else
            {
                datas = datas.OrderByDescending(m => m.CreateTime);
            }
            return datas;
        }

        /// <summary>
        /// 按id取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<T> GetOneByIdAsync(Guid id)
        {
            return await GetAllAsync().FirstAsync(m => m.Id == id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <param name="saved"></param>
        /// <returns></returns>
        public async Task RemoveAsync(T model, bool saved = true)
        {
            await RemoveAsync(model.Id, saved);
        }

        /// <summary>
        /// 按ID删除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="saved"></param>
        /// <returns></returns>
        public async Task RemoveAsync(Guid id, bool saved = true)
        {
            _db.Configuration.ValidateOnSaveEnabled = false;
            var t = new T() { Id = id};
            _db.Entry(t).State = EntityState.Unchanged;
            t.IsRemoved = true;
            if (saved)
            {
                await _db.SaveChangesAsync();
                _db.Configuration.ValidateOnSaveEnabled = true;
            }
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
            _db.Configuration.ValidateOnSaveEnabled = true;
        }
    }
}
