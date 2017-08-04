using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Estellaris.EF.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Estellaris.EF {
  public class BaseRepository<T> : IBaseRepository<T> where T : class {
    protected readonly BaseDbContext DbContext;
    protected readonly DbSet<T> DbSet;
    public bool OutputExceptions { get; set; }

    public BaseRepository(BaseDbContext context) {
      DbContext = context;
      DbSet = context.Set<T>();
    }

    public virtual int Count(Func<T, bool> predicate) {
      return DbSet.AsNoTracking().Count(predicate);
    }

    public virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate) {
      return DbSet.AsNoTracking().CountAsync(predicate);
    }

    public virtual void Delete(Func<T, bool> predicate) {
      Transaction(() => {
        DbContext.RemoveRange(DbSet.Where(predicate));
        DbContext.SaveChanges();
      });
    }

    public virtual void Delete(T entity) {
      if (Object.ReferenceEquals(null, entity))
        return;
      Transaction(() => {
        DbContext.Remove(entity);
        DbContext.SaveChanges();
      });
    }

    public virtual void DeleteAll() {
      Transaction(() => {
        DbContext.RemoveRange(DbSet);
        DbContext.SaveChanges();
      });
    }

    public void DeleteAllAsync() {
      TransactionAsync(async() => {
        DbContext.RemoveRange(DbSet);
        await DbContext.SaveChangesAsync();
      });
    }

    public void DeleteAsync(Func<T, bool> predicate) {
      TransactionAsync(async() => {
        DbContext.RemoveRange(DbSet.Where(predicate));
        await DbContext.SaveChangesAsync();
      });
    }

    public void DeleteAsync(T entity) {
      if (Object.ReferenceEquals(null, entity))
        return;
      TransactionAsync(async() => {
        DbContext.Remove(entity);
        await DbContext.SaveChangesAsync();
      });
    }

    public virtual void Dispose() {
      DbContext.Dispose();
    }

    public virtual IEnumerable<T> Find(Func<T, bool> predicate) {
      return DbSet.AsNoTracking().Where(predicate);
    }

    public virtual IEnumerable<T> FindAll() {
      return DbSet.AsNoTracking();
    }

    public virtual T FindOne(Func<T, bool> predicate) {
      return DbSet.AsNoTracking().FirstOrDefault(predicate);
    }

    public virtual Task<T> FindOneAsync(Expression<Func<T, bool>> predicate) {
      return DbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    public virtual IQueryable<T> NoTrackingQuery() {
      return DbSet.AsNoTracking();
    }

    public virtual IQueryable<T> Query() {
      return DbSet;
    }

    public virtual void Save(T entity) {
      if (Object.ReferenceEquals(null, entity))
        return;
      Transaction(() => {
        DbSet.Add(entity);
        DbContext.SaveChanges();
      });
    }

    public void SaveAsync(T entity) {
      TransactionAsync(async() => {
        DbSet.Add(entity);
        await DbContext.SaveChangesAsync();
      });
    }

    public virtual void SaveRange(params T[] entities) {
      Transaction(() => {
        DbSet.AddRange(entities);
        DbContext.SaveChanges();
      });
    }

    public virtual void SaveRange(IEnumerable<T> entities) {
      SaveRange(entities.ToArray());
    }

    public void SaveRangeAsync(params T[] entities) {
      TransactionAsync(async() => {
        await DbSet.AddRangeAsync(entities);
        await DbContext.SaveChangesAsync();
      });
    }

    public void SaveRangeAsync(IEnumerable<T> entities) {
      SaveRangeAsync(entities.ToArray());
    }

    public virtual void Update(T entity) {
      if (Object.ReferenceEquals(null, entity))
        return;
      Transaction(() => {
        DbSet.Update(entity);
        DbContext.SaveChanges();
      });
    }

    public void UpdateAsync(T entity) {
      if (Object.ReferenceEquals(null, entity))
        return;
      TransactionAsync(async() => {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync();
      });
    }

    public virtual void UpdateRange(params T[] entities) {
      Transaction(() => {
        DbSet.UpdateRange(entities);
        DbContext.SaveChanges();
      });
    }

    public virtual void UpdateRange(IEnumerable<T> entities) {
      UpdateRange(entities.ToArray());
    }

    public void UpdateRangeAsync(params T[] entities) {
      TransactionAsync(async() => {
        DbSet.UpdateRange(entities);
        await DbContext.SaveChangesAsync();
      });
    }

    public void UpdateRangeAsync(IEnumerable<T> entities) {
      UpdateRangeAsync(entities.ToArray());
    }

    async void TransactionAsync(Func<Task> func) {
      using(var transaction = await DbContext.Database.BeginTransactionAsync()) {
        try {
          await func();
          transaction.Commit();
        } catch {
          transaction.Rollback();
        }
      }
    }

    void Transaction(Action func) {
      using(var transaction = DbContext.Database.BeginTransaction()) {
        try {
          func();
          transaction.Commit();
        } catch (Exception ex) {
          if (OutputExceptions)
            Console.WriteLine(ex);
          transaction.Rollback();
        }
      }
    }
  }
}