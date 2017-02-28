using System;
using FluentValidation.Results;

namespace MogBot.Host.Extensions
{
    public static class ValidationExtensions
    {
        public static string FormatError(this ValidationResult result)
        {
            return string.Join(". ", result.Errors);
        }
    }
}