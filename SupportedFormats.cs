using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewer
{
    public enum SupportedFormats
    {
        [RequiresWindowing]Gray8,
        [RequiresWindowing]Gray16,
        [RequiresWindowing] Gray32,
        RGB16,
        RGB32,
        [RequiresConversion]YCbCr_FULL_422,
    }
    
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class RequiresConversionAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class RequiresWindowingAttribute : Attribute
    {
    }
}
