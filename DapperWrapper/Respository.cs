using Dapper;
using DapperExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;
using DapperExtensions.Mapper;

namespace DapperWrapper
{
    /// <summary>
    /// Dapper基本操作的封装，仓库
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class Respository<TEntity> : IRespository where TEntity: class, new()
    {
        protected  IDbContext _context;

        public Respository()
        {
            this._context = new SourceContext();
        }

        public Respository(IDbContext dbContext)
        {
            this._context = dbContext;
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected  dynamic Add(TEntity item)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            UnitOfWork currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if (currentUnitOfWork == null || (currentUnitOfWork != null && currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                IDbConnection writeConnection = this._context.GetConnection();
                object obj2 = null;
                try
                {
                    writeConnection.Open();
                     obj2 = writeConnection.Insert<TEntity>(item, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
                return obj2;
            }
            if (currentUnitOfWork != null)
            {
                if (action == null)
                {
                    action = delegate(IDbConnection conn, IDbTransaction tran, object oitem)
                    {
                        return conn.Insert<TEntity>((TEntity)oitem, tran, null);
                    };
                }
                IClassMapper mapper = DapperExtensions.DapperExtensions.GetMapIncludeForeignKey<TEntity>();
                var properties = mapper.Properties;
                IPropertyMap propertyMap =
                    properties.Where(p => p.Name == "Id").SingleOrDefault(p => p.ColumnName.IndexOf('.') != -1);
                currentUnitOfWork.ChangeOperator(action, this._context, item, propertyMap);
                return null;
            }
            return null;
        }

        /// <summary>
        /// 执行SQL,映射实体
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        protected  void Execute(string sql, dynamic parameters = null)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            UnitOfWork currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if (currentUnitOfWork == null || (currentUnitOfWork != null && currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                IDbConnection writeConnection = this._context.GetConnection();
                try
                {
                    writeConnection.Open();
                    SqlMapper.Execute(writeConnection, sql, (dynamic)parameters);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            if (currentUnitOfWork != null)
            {
                if (action == null)
                {
                    action = (conn, tran,entity) => SqlMapper.Execute(conn, sql, (dynamic) parameters, tran);
                }
                currentUnitOfWork.ChangeOperator(action, this._context);
            }
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns></returns>
        protected  IEnumerable<TEntity> GetAll()
        {
            IEnumerable<TEntity> list = null;
            this.NoLockInvoke(delegate (IDbConnection conn, IDbTransaction tran) {
                list = conn.GetList<TEntity>(null, null, tran, null, false);
            });
            return list;
        }

        /// <summary>
        /// 获取单个标量值
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected  U? GetDataByScalar<U>(string sql, dynamic parameters = null) where U : struct
        {
            if (this.IsChangeSqlString(sql))
            {
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            }
            Nullable<U> value = null;
            Type type = typeof (U);
            this.NoLockInvoke(delegate (IDbConnection conn, IDbTransaction tran)
                                  {
                                      IEnumerable<object> values = SqlMapper.Query(conn, type, sql, (dynamic)parameters, tran);
                                      value = (Nullable<U>)values.SingleOrDefault();
                                  });
            return value;
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected  TEntity GetEntityById(dynamic id)
        {
            TEntity entity = default(TEntity);
            this.NoLockInvoke(delegate (IDbConnection conn, IDbTransaction tran) {
                entity = (TEntity)DapperExtensions.DapperExtensions.Get<TEntity>(conn, id, tran);
            });
            return entity;
        }

        /// <summary>
        /// 根据SQL获取实体列表
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public  IEnumerable<U> GetList<U>(string sql, dynamic parameters = null)
        {
            if (this.IsChangeSqlString(sql))
            {
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            }
            IEnumerable<U> list = null;
            this.NoLockInvoke(delegate(IDbConnection conn, IDbTransaction tran)
            {
                list = (IEnumerable<U>)SqlMapper.Query<U>(conn, sql, (dynamic)parameters, tran);
            });
            return list;
        }

        protected  IEnumerable<TEntity> GetList(string sql, dynamic parameters = null)
        {
            if (this.IsChangeSqlString(sql))
            {
                throw new Exception("sql语句错误,执行的是查询方法,但SQL语句包含更新代码,SQL语句:" + sql);
            }
            IEnumerable<TEntity> list = null;
            this.NoLockInvoke(delegate(IDbConnection conn, IDbTransaction tran)
            {
                list = (IEnumerable<TEntity>)SqlMapper.Query<TEntity>(conn, sql, (dynamic)parameters, tran);
            });
            return list;
        }

        /// <summary>
        /// 判断是否是改变数据库的SQL dcl
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected  bool IsChangeSqlString(string sql)
        {
            sql = sql.Trim().ToLower();
            return (Regex.IsMatch(sql, "^(update|delete|insert|create|alter|drop|truncate)") || Regex.IsMatch(sql, @"^(select)\s+(into)\s"));
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="item"></param>
        protected  void Modify(TEntity item)
        {
            Func<IDbConnection, IDbTransaction,object, object> action = null;
            UnitOfWork currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if (currentUnitOfWork == null || (currentUnitOfWork != null && currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                IDbConnection writeConnection = this._context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Update<TEntity>(item, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                {
                    action = (conn, tran, entity) => conn.Update<TEntity>(item, tran, null);
                }
                currentUnitOfWork.ChangeOperator(action, this._context);
            }
        }

        protected void NoLockInvoke(Action<IDbConnection, IDbTransaction> action)
        {
            using (IDbConnection connection = this._context.GetConnection())
            {
                try
                {
                    connection.Open();
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        action(connection, transaction);
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
        }


        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="item"></param>
        protected  void Remove(TEntity item)
        {
            Func<IDbConnection, IDbTransaction, object, object> action = null;
            UnitOfWork currentUnitOfWork = UnitOfWork.GetCurrentUnitOfWork();
            if (currentUnitOfWork == null || (currentUnitOfWork != null && currentUnitOfWork.Option == UnitOfWorkOption.Suppress))
            {
                IDbConnection writeConnection = this._context.GetConnection();
                try
                {
                    writeConnection.Open();
                    writeConnection.Delete<TEntity>(item, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    writeConnection.Close();
                }
            }
            else if (currentUnitOfWork != null)
            {
                if (action == null)
                {
                    action = (conn, tran,entity) => conn.Delete<TEntity>(item, tran, null);
                }
                currentUnitOfWork.ChangeOperator(action, this._context);
            }
        }
    }
}

