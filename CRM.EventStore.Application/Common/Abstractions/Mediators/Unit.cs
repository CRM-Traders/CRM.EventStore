﻿namespace CRM.EventStore.Application.Common.Abstractions.Mediators;

public readonly struct Unit
{
    public static Unit Value => new();
}