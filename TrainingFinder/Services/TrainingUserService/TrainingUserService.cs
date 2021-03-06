﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using TrainingFinder.Data;
using TrainingFinder.Dtos.Training;
using TrainingFinder.Dtos.TrainingUser;
using TrainingFinder.Entities;
using TrainingFinder.Models;

namespace TrainingFinder.Services.TrainingUserService
{
    public class TrainingUserService : ITrainingUserService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public TrainingUserService(ApplicationDbContext ctx, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository)
        {
            _ctx = ctx;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }

        public ResultModel<GetTrainingDto> AddTrainingUser(AddTrainingUserDto newTrainingUser)
        {
            /*ServiceResponse<GetTrainingDto> response = new ServiceResponse<GetTrainingDto>();*/

            try
            {
                Training training = _ctx.Trainings.FirstOrDefault(t => t.TrainingId == newTrainingUser.TrainingId);
                if (training == null)
                {
                    return new ResultModel<GetTrainingDto>(null, 404);
                }

                User user = _ctx.Users.FirstOrDefault(u => u.Id == newTrainingUser.UserId);
                if (user == null)
                {
                    return new ResultModel<GetTrainingDto>(null, 404);
                }

                TrainingUser trainingUser = new TrainingUser
                {
                    Training = training,
                    User = user
                };

                _ctx.TrainingUsers.Add(trainingUser);
                _ctx.SaveChanges();

                return new ResultModel<GetTrainingDto>(null, 201);
            }
            catch (Exception)
            {
                return new ResultModel<GetTrainingDto>(null, 404);
            }
        }

        public ResultModel<List<GetTrainingDtoWithoutUsers>> GetUserTrainings(int id)
        {
            try
            {
                var user = _userRepository.GetById(id);
                if (user.Data == null)
                {
                    return new ResultModel<List<GetTrainingDtoWithoutUsers>>(null, StatusCodes.Status404NotFound);
                }

                if (!user.isStatusCodeSuccess())
                {
                    return new ResultModel<List<GetTrainingDtoWithoutUsers>>(null, StatusCodes.Status400BadRequest);
                }

                var trainings = _ctx.TrainingUsers.Where(c => c.UserId == id).Select(c => c.Training).ToList();
                var model = _mapper.Map<List<GetTrainingDtoWithoutUsers>>(trainings);

                if (!trainings.Any())
                {
                    return new ResultModel<List<GetTrainingDtoWithoutUsers>>(null, StatusCodes.Status404NotFound);
                }

                return new ResultModel<List<GetTrainingDtoWithoutUsers>>(model, StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                return new ResultModel<List<GetTrainingDtoWithoutUsers>>(null, StatusCodes.Status500InternalServerError);
            }
        }
    }
}