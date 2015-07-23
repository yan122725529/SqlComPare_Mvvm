using System;
using System.Collections.Generic;
using System.Linq;
using AutoFacWrapper;
using SqliteCompare.Entity;
using SqliteCompare.Repository;
using SqliteCompare.Service.InterFace;

namespace SqliteCompare.Service
{
    public class CompareService : ICompareService
    {
        private readonly ISourceRepository _sourceRepository;
        private readonly ITargetRepository _TargetRepository;
        private List<IndexCompareResult> _difIndexList;
        private List<TableCompareResult> _difTableList;

        public CompareService()
        {
            _sourceRepository = ClassFactory.GetInstance<ISourceRepository>();
            _TargetRepository = ClassFactory.GetInstance<ITargetRepository>();
        }

        /// <summary>
        ///     表结构对比结果
        /// </summary>
        public List<TableCompareResult> DifTableList
        {
            set { _difTableList = value; }
            get
            {
                if (_difTableList == null)
                    CompareDBTable();
                return _difTableList;
            }
        }

        /// <summary>
        ///     索引对比结果
        /// </summary>
        public List<IndexCompareResult> DifIndexList
        {
            set { _difIndexList = value; }
            get
            {
                if (_difIndexList == null)
                    CompareDBIndex();
                return _difIndexList;
            }
        }

        /// <summary>
        ///     对比表结构
        /// </summary>
        /// <returns></returns>
        public List<TableCompareResult> CompareDBTable()
        {
            RefreshContext();
            //todo 排除演示数据的表
            var source = _sourceRepository.GetDBTables();
            var target = _TargetRepository.GetDBTables();

            //对象存在，但是DDLSQL不一致
            var difList = (from s in source
                from t in target.Where(t => s.name == t.name && s.type == t.type && s.sql != t.sql)
                select new Tuple<DbObjectInfo, DbObjectInfo>(s, t)).ToList();
            //source存在target不存在存在=>标准库有，目标库没有，属于少的
            var lostList = source.Where(t => !target.Any(s => t.name == s.name && t.type == s.type)).ToList();
            //target存在source 不存在  =>标准库没有，属于多的
            var moreList = target.Where(t => !source.Any(s => t.name == s.name && t.type == s.type)).ToList();


            //加入到结果列表
            var ResultList =
                difList.Select(
                    info => new TableCompareResult {SourceInfo = info.Item1, TargetInfo = info.Item1, ErrorType = 1})
                    .ToList();
            ResultList.AddRange(
                lostList.Select(info => new TableCompareResult {SourceInfo = info, TargetInfo = null, ErrorType = 2}));
            ResultList.AddRange(
                moreList.Select(info => new TableCompareResult {SourceInfo = null, TargetInfo = info, ErrorType = 3}));
            //找出不同的列，并根据列的信息来生成修复语句
            BatchGetDifColByTable(ResultList);
            _difTableList = ResultList;
            return ResultList;
        }

        /// <summary>
        ///     对比索引结构
        /// </summary>
        /// <returns></returns>
        public List<IndexCompareResult> CompareDBIndex()
        {
            //todo 排除演示数据的表
            var source = _sourceRepository.GetDBIndex();
            var target = _TargetRepository.GetDBIndex();

            //对象存在，但是DDLSQL不一致
            var difList = (from s in source
                from t in target.Where(t => s.name == t.name && s.type == t.type && s.sql != t.sql)
                select new Tuple<DbObjectInfo, DbObjectInfo>(s, t)).ToList();
            //source存在target不存在存在=>标准库有，目标库没有，属于少的
            var lostList = source.Where(t => !target.Any(s => t.name == s.name && t.type == s.type)).ToList();
            //target存在source 不存在  =>标准库没有，属于多的
            var moreList = target.Where(t => !source.Any(s => t.name == s.name && t.type == s.type)).ToList();


            //加入到结果列表
            var ResultList =
                difList.Select(
                    info => new IndexCompareResult {SourceInfo = info.Item1, TargetInfo = info.Item1, ErrorType = 1})
                    .ToList();

            ResultList.AddRange(
                lostList.Select(info => new IndexCompareResult {SourceInfo = info, TargetInfo = null, ErrorType = 2}));
            ResultList.AddRange(
                moreList.Select(info => new IndexCompareResult {SourceInfo = null, TargetInfo = info, ErrorType = 3}));
            _difIndexList = ResultList;
            return ResultList;
        }

        public void RefreshContext()
        {
            _sourceRepository.RefreshContext();
            _TargetRepository.RefreshContext();
        }

        public void RepairDb(IEnumerable<string> sqls, Action<int> change)
        {
            _TargetRepository.RepairDb(sqls,change);
        }

        /// <summary>
        ///     找出不同的列，并根据列的信息来生成修复语句
        /// </summary>
        /// <param name="info"></param>
        public void BatchGetDifColByTable(List<TableCompareResult> tableInfos)
        {
            var difinfo = tableInfos.Where(x => x.ErrorType == 1).ToList();
            if (!difinfo.Any())
                return;
            foreach (var info in difinfo)
            {
                var sourcecol = _sourceRepository.GetColInfoByTable(info.SourceInfo.name).ToList();
                var targetcol = _TargetRepository.GetColInfoByTable(info.TargetInfo.name).ToList();

                if (!(sourcecol != null && targetcol != null))
                    continue;


                var lostList = new List<SqliteColInfo>();
                var moreList = new List<SqliteColInfo>();
                /*
                //对象存在，但是DDLSQL不一致
                //目前咱不考虑列的其他信息不一致的情况，这种情况建议是删掉重新建，但是客户数据又不要处理，所以暂不考虑
                var difList = (from s in sourcecol
                    from t in
                        targetcol.Where(
                            t =>
                                s.name == t.name &&
                                (s.type != t.type || s.NotNull != t.NotNull || s.Dflt_Value != t.Dflt_Value ||
                                 s.Pk != t.typePk))
                    select new Tuple<SqliteColInfo, SqliteColInfo>(s, t)).ToList();
                //先删后建
                foreach (var dif in difList)
                {
                    moreList.Add(dif.Item1);
                    lostList.Add(dif.Item2);
                }*/
                //source存在target不存在存在=>标准库有，目标库没有，属于少的
                lostList.AddRange(sourcecol.Where(t => targetcol.All(s => t.Name != s.Name)));
                //target存在source 不存在  =>标准库没有，属于多的
                moreList.AddRange(targetcol.Where(t => sourcecol.All(s => t.Name != s.Name)));

                info.SourceCol = sourcecol;
                info.TargetCol = targetcol;

                info.LostCol = lostList;
                info.MoreCol = moreList;
            }
        }
    }
}