namespace DTerminal.Core.Gear
{
    /// <summary>
    /// 圆柱斜齿轮
    /// </summary>
    public class CylindricalHelicalGear
    {

        /// <summary>
        /// 分度圆直径 m
        /// </summary>
        public double DividingCircle { get; set; } = 200;
        /// <summary>
        /// 压力角 deg
        /// </summary>
        public double PressureAngle { get; set; } = 20;
        /// <summary>
        /// 螺旋角 deg
        /// </summary>
        public double HelixAngle { get; set; } = 18;


        /// <summary>
        /// 计算啮合力
        /// </summary>
        /// <param name="gear"></param>
        /// <param name="torsion">扭矩</param>
        /// <returns></returns>
        public MeshingForce GetMeshingForce(double torsion)
        {
            MeshingForce force = new MeshingForce();
            force.Tangential = 2 * torsion / DividingCircle;
            force.Axial = force.Tangential * HelixAngle.Tan();
            force.Radial = force.Tangential * PressureAngle.Tan() / HelixAngle.Cos();
            force.Normal = force.Tangential / (PressureAngle.Cos() * HelixAngle.Cos());
            return force;
        }


    }
}
