using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Estellaris.EF.Interfaces {
  public interface IBaseRepository<T> : IDisposable {
    int Count();
    int Count(Func<T, bool> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    void Delete(Func<T, bool> predicate);
    void DeleteAsync(Func<T, bool> predicate);
    void Delete(T entity);
    void DeleteAsync(T entity);
    void DeleteAll();
    void DeleteAllAsync();
    IEnumerable<T> Find(Func<T, bool> predicate);
    IEnumerable<T> FindAll();
    T FindOne(Func<T, bool> predicate);
    Task<T> FindOneAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> NoTrackingQuery();
    IQueryable<T> Query();
    void Save(T entity);
    void SaveAsync(T entity);
    void SaveRange(params T[] entities);
    void SaveRangeAsync(params T[] entities);
    void SaveRange(IEnumerable<T> entities);
    void SaveRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateAsync(T entity);
    void UpdateRange(params T[] entities);
    void UpdateRangeAsync(params T[] entities);
    void UpdateRange(IEnumerable<T> entities);
    void UpdateRangeAsync(IEnumerable<T> entities);
  }
}