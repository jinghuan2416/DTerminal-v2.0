using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Optistruct
{
    public class FrequencyResponseParameter
    {

        /// <summary>
        /// 是否启用工况组合
        /// </summary>
        public bool IsCombineLoad { get; set; } = true;


        /// <summary>
        /// 扫频步长
        /// </summary>
        public int NumberofFrequencySweeps { get; set; } = 50;

        /// <summary>
        /// FREQ3参数
        /// </summary>
        public double FRQE3NEF { get; set; } = 10;

        /// <summary>
        /// FREQ3参数
        /// </summary>
        public double FRQE3CLUSTER { get; set; } = 3;



        /// <summary>
        /// 节点总数
        /// </summary>
        public int NodeCount { get; set; } = 12;
        /// <summary>
        /// 开始频率
        /// </summary>
        public double StarFrequency { get; set; } = 0.1;
        /// <summary>
        /// 目标频率
        /// </summary>
        public double TargetFrequency { get; set; } = 7200;
        /// <summary>
        /// 全局阻尼
        /// </summary>
        public double ParamG { get; set; } = 0.04;


        /// <summary>
        /// 开始节点编号
        /// </summary>
        public int StarNode { get; set; } = 1;
        /// <summary>
        /// Tabled1卡片编号
        /// </summary>
        public int Tabled1ID { get; set; } = 10000;

        /// <summary>
        /// SET卡片起始编号
        /// </summary>
        public int SetGridID { get; set; } = 30000 + 1;

        /// <summary>
        /// 工况卡片起始编号
        /// </summary>
        public int SubcaseID { get; set; } = 30000;

        /// <summary>
        /// EIGRL卡片编号
        /// </summary>
        public int EigrlID { get; set; } = 10001;

        /// <summary>
        /// EFREQ卡片编号
        /// </summary>
        public int FreqID { get; set; } = 10002;

        /// <summary>
        /// RLOAD卡片起始编号
        /// </summary>
        public int RloadID { get; set; } = 30000 + 1;

        /// <summary>
        /// DAREA卡片起始编号
        /// </summary>
        public int DareaID { get; set; } = 30000 + 10001;
    }
}
