using Microsoft.EntityFrameworkCore;
using CustomSpectreConsole;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FriendManager.DAL.Base;
using System.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using FriendManager.BAL.Discord;

namespace FriendManager.BAL.Base
{
    public abstract class BALObjectBase<TDal, TContext>
    where TDal : class, new()
    where TContext : ContextBase, new()
    {
        #region Properties

        protected TDal _item;
        protected bool IsNew { get; set; }
        public bool LoadComplete { get; protected set; }
        public Task Loading { get; }

        #endregion

        #region Life Cycle

        public BALObjectBase()
        {
            IsNew = true;
            LoadComplete = true;
            Loading = Task.CompletedTask;
            _item = new TDal();
        }

        public BALObjectBase(object id)
        {
            Loading = Load(id);
        }

        public BALObjectBase(TDal item)
        {
            IsNew = item == null;
            LoadComplete = true;
            Loading = Task.CompletedTask;
            _item = item ?? new TDal();
        }

        protected async Task Load(object primaryKeyValue)
        {
            _item = await FetchInstance(primaryKeyValue);

            if(_item == null)
            {
                IsNew = true;
                _item = new TDal();
            }
            else
            {
                IsNew = false;
            }

            LoadComplete = true;
            await Task.CompletedTask;
        }

        #endregion

        #region Private API

        protected async Task InternalSave()
        {
            using (TContext context = new TContext())
            {
                if(IsNew)
                    await context.AddAsync(_item);

                await context.SaveChangesAsync().AwaitTimeout();
            }

            IsNew = false;

            await Task.CompletedTask;
        }

        private static async Task<TDal> FetchInstance(object primaryKeyValue)
        {
            TDal item;

            using (TContext context = new TContext())
            {
                PropertyInfo primaryKeyProp = typeof(TDal).GetProperties()
                    .FirstOrDefault(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(KeyAttribute)));

                if (primaryKeyProp == null)
                    throw new Exception(string.Format("The entity '{0}' lacks a primary key", nameof(TContext)));

                DbSet<TDal> entites = context.GetInstances<TDal>();

                var param = Expression.Parameter(typeof(TDal), "x");
                var prop = Expression.Property(param, primaryKeyProp.Name);
                var value = Expression.Constant(Convert.ChangeType(primaryKeyValue, primaryKeyProp.PropertyType));

                BinaryExpression equal = Expression.Equal(prop, value);

                var lambda = Expression.Lambda<Func<TDal, bool>>(equal, param);
                item = await entites.Where(lambda).FirstOrDefaultAsync();
            }

            return item;
        }

        protected static async Task<List<TDal>> FetchInstances(List<Expression<Func<TDal, bool>>> clauses, List<Expression<Func<TDal, object>>> includes = null)
        {
            List<TDal> items = new List<TDal>();

            using (TContext context = new TContext())
            {
                DbSet<TDal> entites = context.GetInstances<TDal>();

                IQueryable<TDal> query = entites.AsQueryable();

                if (includes != null)
                {
                    includes.ForEach(x =>
                    {
                        bool valid = x.Body.Type.IsCompatible(typeof(DALObjectBase));

                        if (valid)
                            query = query.Include(x);
                    });
                }

                if (clauses != null)
                    clauses.ForEach(x => query = query.Where(x));

                items = await query.ToListAsync();
            }

            return items;
        }

        #endregion

        #region Static API

        internal static async Task<List<T>> GetAll<T>(List<Expression<Func<TDal, bool>>> clauses = null, List<Expression<Func<TDal, object>>> includes = null)
        where T : new()
        {
            List<TDal> items = await FetchInstances(clauses, includes);
            List<T> models = new List<T>();

            foreach (var item in items)
                models.Add((T)Activator.CreateInstance(typeof(T), new object[] { item }));

            return models;
        }

        #endregion
    }
}
