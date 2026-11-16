using System;
using System.Globalization;
using System.Web.Mvc;

namespace Web.Helpers
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(
            ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            bool isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null)
            {
                if (!isNullable)
                {
                    bindingContext.ModelState.AddModelError(
                        bindingContext.ModelName,
                        "Vui lòng nhập giá trị số.");
                    return decimal.Zero;
                }

                return null;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
            var input = value.AttemptedValue;
            if (string.IsNullOrWhiteSpace(input) && isNullable)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    "Vui lòng nhập giá trị số.");
                return decimal.Zero;
            }

            decimal result;
            if (TryParse(input, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }

            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                "Vui lòng nhập giá trị số hợp lệ.");
            return isNullable ? (object)null : decimal.Zero;
        }

        public static bool TryParse(
            string input,
            CultureInfo culture,
            out decimal value)
        {
            value = decimal.Zero;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();
            int dotIndex = input.LastIndexOf('.');
            int commaIndex = input.LastIndexOf(',');
            if (dotIndex >= 0 && commaIndex >= 0)
            {
                var inputCulture = dotIndex > commaIndex
                    ? CultureInfo.InvariantCulture
                    : culture;
                return decimal.TryParse(
                    input,
                    NumberStyles.Number,
                    inputCulture,
                    out value);
            }

            var cultureToUse = dotIndex >= 0
                ? CultureInfo.InvariantCulture
                : culture;
            return decimal.TryParse(
                input,
                NumberStyles.Number,
                cultureToUse,
                out value);
        }
    }
}
