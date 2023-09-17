using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Gear
{
    public struct MeshingForce
    {
        /// <summary>
        /// 切向
        /// </summary>
        public double Tangential { get; set; }

        /// <summary>
        /// 径向
        /// </summary>
        public double Radial { get; set; }

        /// <summary>
        /// 轴向
        /// </summary>
        public double Axial { get; set; }

        /// <summary>
        /// 法向
        /// </summary>
        public double Normal { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"径向力：{Radial} N");
            sb.AppendLine($"切向力：{Tangential} N");
            sb.AppendLine($"轴向力：{Axial} N");
            sb.AppendLine($"法向量：{Normal} N");
            return sb.ToString();
        }
    }
}
