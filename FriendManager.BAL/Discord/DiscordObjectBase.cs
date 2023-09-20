using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FriendManager.BAL.Base;
using FriendManager.DAL.Base;
using FriendManager.DAL.Discord;
using FriendManager.DAL.Discord.Models;
using Microsoft.EntityFrameworkCore;

namespace FriendManager.BAL.Discord
{
    public abstract class DiscordObjectBase<TBal, TDal> : BALObjectBase<TDal, DiscordContext>
    where TBal : class, new()
    where TDal : class, new()
    {
        #region Life Cycle

        public DiscordObjectBase()
        : base() { }

        public DiscordObjectBase(object primaryKeyValue)
        : base(primaryKeyValue) { }

        public DiscordObjectBase(TDal item)
        : base(item) { }

        #endregion

        #region Public API

        public virtual async Task Save()
        {
            await InternalSave();
        }

        public virtual async Task Delete()
        {
            await InternalDelete();
        }

        #endregion

        #region Static API

        public static async Task<List<TBal>> GetAll(List<Expression<Func<TDal, bool>>> clauses = null, List<Expression<Func<TDal, object>>> includes = null)
        {
            List<TBal> items = await GetAll<TBal>(clauses, includes);
            return items;
        }

        #endregion
    }
}
