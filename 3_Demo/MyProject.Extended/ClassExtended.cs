using MyCompany.MyProject.A_SetInProject;
using MyCompany.MyProject.B_SetInDirectoryBuildProps;
using MyCompany.MyProject.C_SetInNuGetPackage;

namespace MyCompany.MyProject.Extended.SetInProject
{
    public class ClassExtended
    {
        /// <summary>
        /// A meaningless property to act like we would be using something from the project MyProject.A_SetInProject
        /// </summary>
        public ClassA? SomeUsageOfClassA { get; set; }

        /// <summary>
        /// A meaningless property to act like we would be using something from the project MyProject.B_SetInDirectoryBuildProps
        /// </summary>
        public ClassB? SomeUsageOfClassB { get; set; }

        /// <summary>
        /// A meaningless property to act like we would be using something from the project MyProject.C_SetInNuGetPackage
        /// </summary>
        public ClassC? SomeUsageOfClassC { get; set; }
    }
}