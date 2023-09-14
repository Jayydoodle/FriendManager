using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FriendManager.DAL.Base
{
    public abstract class ContextBase : DbContext
    {
        #region Constants

        public const string ConnectionStringName = "FriendTech";

        #endregion

        #region Public API

        public DbSet<T> GetInstances<T>()
        where T : class
        {
            Type type = typeof(DbSet<>).MakeGenericType(typeof(T));

            PropertyInfo prop = GetType().GetProperties().FirstOrDefault(x => x.PropertyType == type);

            if (prop == null)
                throw new Exception(string.Format("The type '{0}' does not exist in the context '{1}'", nameof(T), GetType().Name));

            object instances = prop.GetValue(this);

            return instances as DbSet<T>;
        }

        #endregion
    }
}
