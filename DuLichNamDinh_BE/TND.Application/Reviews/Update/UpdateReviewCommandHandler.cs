﻿using AutoMapper;
using MediatR;
using TND.Application.Reviews.Common;
using TND.Domain.Entities;
using TND.Domain.Exceptions;
using TND.Domain.Interfaces.Persistence;
using TND.Domain.Interfaces.Persistence.Repositories;
using TND.Domain.Messages;

namespace TND.Application.Reviews.Update
{
    public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateReviewCommandHandler(IHotelRepository hotelRepository, IUserRepository userRepository,
            IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            if (!await _hotelRepository.ExistsByIdAsync(request.HotelId, cancellationToken))
            {
                throw new NotFoundException(HotelMessages.NotFound);
            }
            if (!await _userRepository.ExistsByIdAsync(request.GuestId, cancellationToken))
            {
                throw new NotFoundException(UserMessages.NotFound);
            }

            var review = await _reviewRepository
                .GetByIdAsync(request.ReviewId, request.HotelId, request.GuestId, cancellationToken)
                ?? throw new NotFoundException(ReviewMessages.NotFoundForUserForHotel);

            var ratingSum = await _reviewRepository.GetTotalRatingForHotelAsync(request.HotelId, cancellationToken);

            var reviewsCount = await _reviewRepository.GetReviewCountForHotelAsync(request.HotelId, cancellationToken);

            ratingSum += request.Rating - review.Rating;

            var newRating = 1.0 * ratingSum / reviewsCount;

            await _hotelRepository.UpdateReviewById(request.HotelId, newRating, cancellationToken);

            await _reviewRepository.UpdateAsync(_mapper.Map(request, review), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
