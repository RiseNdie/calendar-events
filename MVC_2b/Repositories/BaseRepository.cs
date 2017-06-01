using MVC_2b.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace MVC_2b.Repositories
{
    public class BaseRepository<T> where T : BaseEntityWithId
    {
        public DbContext Context { get; set; }
        public DbSet<T> DbSet { get; set; }

        public BaseRepository()
        {
            this.Context = new MVCContext();
            this.DbSet = this.Context.Set<T>();
        }

        public virtual List<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> result = DbSet;
            if (filter != null)
            {
                return result.Where(filter).ToList();
            }
            else
            {
                return result.ToList();
            }
        }

        public T GetById(int id)
        {
            return DbSet.Find(id);
        }

        public virtual void Save(T item)
        {
            if (item.Id <= 0)
            {
                Insert(item);
            }
            else
            {
                Update(item);
            }
        }
        private void Insert(T item)
        {
            this.DbSet.Add(item);
            this.Context.SaveChanges();
        }

        private void Update(T item)
        {
            this.Context.Entry(item).State = EntityState.Modified;
            this.Context.SaveChanges();
        }

        public virtual void Delete(T item)
        {
            this.DbSet.Remove(item);
            this.Context.SaveChanges();
        }
    }
}