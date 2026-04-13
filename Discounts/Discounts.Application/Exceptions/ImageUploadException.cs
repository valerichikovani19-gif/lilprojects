// Copyright (C) TBC Bank. All Rights Reserved.
namespace Discounts.Application.Exceptions
{
    public class ImageUploadException : BadRequestException
    {
        public ImageUploadException(string message) : base(message)
        {
        }
    }
}
