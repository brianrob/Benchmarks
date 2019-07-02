// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Ignitor.RenderTree;

namespace Ignitor.Forms
{
    /// <summary>
    /// An input component for editing date values.
    /// Supported types are <see cref="DateTime"/> and <see cref="DateTimeOffset"/>.
    /// </summary>
    public class InputDate<T> : InputBase<T>
    {
        const string dateFormat = "yyyy-MM-dd"; // Compatible with HTML date inputs

        /// <summary>
        /// Gets or sets the error message used when displaying an a parsing error.
        /// </summary>
        [Parameter] public string ParsingErrorMessage { get; private set; } = "The {0} field must be a date.";

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "input");
            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddAttribute(2, "type", "date");
            builder.AddAttribute(3, "id", Id);
            builder.AddAttribute(4, "class", CssClass);
            builder.AddAttribute(5, "value", BindMethods.GetValue(CurrentValueAsString));
            builder.AddAttribute(6, "onchange", BindMethods.SetValueHandler(__value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.CloseElement();
        }

        /// <inheritdoc />
        protected override string FormatValueAsString(T value)
        {
            switch (value)
            {
                case DateTime dateTimeValue:
                    return dateTimeValue.ToString(dateFormat);
                case DateTimeOffset dateTimeOffsetValue:
                    return dateTimeOffsetValue.ToString(dateFormat);
                default:
                    return string.Empty; // Handles null for Nullable<DateTime>, etc.
            }
        }

        /// <inheritdoc />
        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {
            // Unwrap nullable types. We don't have to deal with receiving empty values for nullable
            // types here, because the underlying InputBase already covers that.
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            bool success;
            if (targetType == typeof(DateTime))
            {
                success = TryParseDateTime(value, out result);
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                success = TryParseDateTimeOffset(value, out result);
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
            }

            if (success)
            {
                validationErrorMessage = null;
                return true;
            }
            else
            {
                validationErrorMessage = string.Format(ParsingErrorMessage, FieldIdentifier.FieldName);
                return false;
            }
        }

        static bool TryParseDateTime(string value, out T result)
        {
            var success = DateTime.TryParse(value, out var parsedValue);
            if (success)
            {
                result = (T)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        static bool TryParseDateTimeOffset(string value, out T result)
        {
            var success = DateTimeOffset.TryParse(value, out var parsedValue);
            if (success)
            {
                result = (T)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}
