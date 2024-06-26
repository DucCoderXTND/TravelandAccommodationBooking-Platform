﻿using MediatR;

namespace TND.Application.RoomClasses.Delete
{
    public record DeleteRoomClassCommand(Guid RoomClassId) : IRequest;
    
}
