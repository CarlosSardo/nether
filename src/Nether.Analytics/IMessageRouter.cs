﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Nether.Analytics
{
    public interface IMessageRouter
    {
        //TODO: Make method Async
        Task RouteMessageAsync(Message msg);
    }
}