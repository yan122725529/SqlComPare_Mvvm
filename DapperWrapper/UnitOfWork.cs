using System.Reflection;
using DapperExtensions.Mapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;
using Dapper;
using DapperExtensions;

namespace DapperWrapper
{
    /// <summary>
    /// 事务的封装
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private bool isShadow;
        private List<OperatorInfo> list;
        private List<Map> Maplist;
        private static object lockObj = new object();
        //[ThreadStatic]
        private static ConcurrentStack<UnitOfWork> unitOfWorkStack = null;

        public UnitOfWork() : this(UnitOfWorkOption.Required)
        {
        }

        public UnitOfWork(UnitOfWorkOption option)
        {
            list = new List<OperatorInfo>();
            Maplist = new List<Map>();
            this.isShadow = false;
            this.Option = option;
            if (this.Option == UnitOfWorkOption.Required)
            {
                UnitOfWork currentUnitOfWork = GetCurrentUnitOfWork();
                if (currentUnitOfWork == null)
                {
                    this.list = new List<OperatorInfo>();
                }
                else
                {
                    this.list = currentUnitOfWork.list;
                    this.isShadow = true;
                }
            }
            UnitOfWorkStack.Push(this);
        }

        public void ChangeOperator(Func<IDbConnection, IDbTransaction,object, object> action, IDbContext context, dynamic entity,IPropertyMap map=null)
        {
            OperatorInfo item = new OperatorInfo {
                func = action,
                Context = context,
                entity = entity,
                Map = map
            };
            this.list.Add(item);
        }

        public void ChangeOperator(Func<IDbConnection, IDbTransaction, object, object> action, IDbContext context)
        {
            OperatorInfo item = new OperatorInfo
            {
                func = action,
                Context = context,
            };
            this.list.Add(item);
        }

        private void Commit()
        {
            IDbConnection connection;
            switch ((from p in this.list select p.Context.ContextName).Distinct<string>().Count<string>())
            {
                case 0:
                    break;

                case 1:
                    using (connection = this.list.First<OperatorInfo>().Context.GetConnection())
                    {
                        connection.Open();
                        using (IDbTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (OperatorInfo info in this.list)
                                {
                                    object entity = MapForeignKey(info);
                                    object id = info.func(connection, transaction, entity);
                                    if (info.Map != null)
                                    {
                                        string[] strs = info.Map.ColumnName.Split('.');
                                        Map map = new Map() {id = id, MapTablename = strs[0], MapTableCol = strs[1]};
                                        Maplist.Add(map);
                                    }
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            finally
                            {
                                connection.Close();
                                this.list.Clear();
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理外键ID
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private object MapForeignKey(OperatorInfo info)
        {
            if (info.entity == null)
                return null;
            Map map = Maplist.SingleOrDefault(p => p.MapTablename == info.entity.GetType().Name);
            if (map == null)
             return info.entity;
            try
            {
                PropertyInfo pi =
              info.entity.GetType().GetProperties().SingleOrDefault(p => p.Name == map.MapTableCol);
                if (pi != null)
                {
                    pi.SetValue(info.entity, map.id, null);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("{0} mapping for property {1} failed.", map.MapTablename,map.MapTableCol));
            }
            return info.entity;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Complete()
        {
            UnitOfWork currentUnitOfWork = GetCurrentUnitOfWork();
            if (currentUnitOfWork != this)
            {
                throw new Exception("当前提交的事务内还有未完成的其他事务,请先完成内部的事务");
            }
            if ((this.Option == UnitOfWorkOption.RequiresNew) || ((this.Option == UnitOfWorkOption.Required) && !this.isShadow))
            {
                this.Commit();
            }
            UnitOfWorkStack.TryPop(out currentUnitOfWork);
            this.Dispose();
        }

        public void Dispose()
        {
            if (!this.isShadow)
            {
                this.list.Clear();
            }
            UnitOfWork currentUnitOfWork = GetCurrentUnitOfWork();
            if (currentUnitOfWork == this)
            {
                UnitOfWorkStack.TryPop(out currentUnitOfWork);
            }
        }

        public static UnitOfWork GetCurrentUnitOfWork()
        {
            UnitOfWork result = null;
            UnitOfWorkStack.TryPeek(out result);
            return result;
        }

        public void SetCurrentUnitOfWork(UnitOfWork unitOfWork)
        {
            UnitOfWorkStack.Push(unitOfWork);
        }

        public UnitOfWorkOption Option { get; private set; }

        /// <summary>
        /// 事务堆栈
        /// </summary>
        private static ConcurrentStack<UnitOfWork> UnitOfWorkStack
        {
            get
            {
                if (unitOfWorkStack == null)
                {
                    lock (lockObj)
                    {
                        if (unitOfWorkStack == null)
                        {
                            unitOfWorkStack = new ConcurrentStack<UnitOfWork>();
                        }
                    }
                }
                return unitOfWorkStack;
            }
        }
    }
}

