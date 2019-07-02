// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Ignitor.RenderTree;

namespace Ignitor.Forms
{
    // TODO: Support maxlength etc.

    /* This is almost equivalent to a .razor file containing:
     *
     *    @inherits InputBase<string>
     *    <input @bind="@CurrentValue" id="@Id" class="@CssClass" />
     *
     * The only reason it's not implemented as a .razor file is that we don't presently have the ability to compile those
     * files within this project. Developers building their own input components should use Razor syntax.
     */

    /// <summary>
    /// An input component for editing <see cref="string"/> values.
    /// </summary>
    public class InputText : InputBase<string>
    {
        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "id", Id);
            builder.AddAttribute(3, "class", CssClass);
            builder.AddAttribute(4, "value", BindMethods.GetValue(CurrentValue));
            builder.AddAttribute(5, "onchange", BindMethods.SetValueHandler(__value => CurrentValue = __value, CurrentValue));
            builder.CloseElement();
        }

        /// <inheritdoc />
        protected override bool TryParseValueFromString(string value, out string result, out string validationErrorMessage)
        {
            result = value;
            validationErrorMessage = null;
            return true;
        }
    }
}
