﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TrainingFinder.Models;

namespace TrainingFinder.Data
{
    public class GymRepository : IGymRepository
    {
        private readonly ApplicationDbContext _ctx;
        public GymRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }
        public IQueryable<Gym> Gyms => _ctx.Gyms;
        public bool DeleteGym(int id)
        {
            var entityToDelete = _ctx.Gyms.FirstOrDefault(g => g.GymId == id);

            if (entityToDelete != null)
            {
                _ctx.Gyms.Remove(entityToDelete);
                _ctx.SaveChanges();
                return true;
            }
            else
                return false;

        }
        public bool SaveGym(Gym entity)
        {
            if (entity.GymId == 0)
            {
                _ctx.Gyms.Add(entity);
                _ctx.SaveChanges();
                return true;
            }
            else if (entity.GymId != 0)
            {
                var entityToUpdate = _ctx.Gyms.FirstOrDefault(g => g.GymId == entity.GymId);

                entityToUpdate.Name = entity.Name;
                entityToUpdate.City = entity.City;
                entityToUpdate.Street = entity.Street;
                entityToUpdate.Number = entity.Number;
                entityToUpdate.PostCode = entity.PostCode;
                entityToUpdate.Trainings = entity.Trainings;
                entityToUpdate.IsAddedByUser = entity.IsAddedByUser;
                entityToUpdate.Latitude = entity.Latitude;
                entityToUpdate.Longitude = entity.Longitude;

                if (entityToUpdate == null)
                    return false;

                _ctx.SaveChanges();
                return true;
            }
            return false;
        }


        public ResultModel<Gym> Create(Gym gym)
        {
            try
            {
                if (gym.GymId == 0)
                {
                    _ctx.Gyms.Add(gym);
                    _ctx.SaveChanges();
                    return new ResultModel<Gym>(gym, 201);
                }
                else
                {
                    if (gym != null)
                        return new ResultModel<Gym>(gym, 409);
                }
                return new ResultModel<Gym>(gym, 409);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ResultModel<Gym>(null, 500);
            }
        }

        public ResultModel<Gym> Delete(int id)
        {
            try
            {
                var entityToDelete = _ctx.Gyms.FirstOrDefault(x => x.GymId == id);

                if (entityToDelete == null)
                    return new ResultModel<Gym>(null, 404);

                _ctx.Gyms.Remove(entityToDelete);
                _ctx.SaveChanges();
                return new ResultModel<Gym>(null, 204);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ResultModel<Gym>(null, 500);
            }
        }       
        

        public ResultModel<IEnumerable<Gym>> GetAllInCity(string city)
        {
            try
            {
                IEnumerable<Gym> gyms;
                if (string.IsNullOrWhiteSpace(city))
                    gyms = _ctx.Gyms.ToList();
                else
                    gyms = _ctx.Gyms.Where(x => x.City == city).ToList();

                return new ResultModel<IEnumerable<Gym>>(gyms, 200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ResultModel<IEnumerable<Gym>>(null, 500);
            }
        }

        public ResultModel<Gym> GetById(int id)
        {
            try
            {
                var entity = _ctx.Gyms.FirstOrDefault(x => x.GymId == id);

                if (entity == null)
                    return new ResultModel<Gym>(null, 404);
                else
                    return new ResultModel<Gym>(entity, 200);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ResultModel<Gym>(null, 500);
            }
        }       

        public ResultModel<Gym> Update(Gym gym)
        {
            try
            {
                var getResponse = _ctx.Gyms.
                    Where(g => g.GymId == gym.GymId)
                    .Include(g => g.Trainings)
                    .SingleOrDefault();

                if (getResponse == null)
                    return new ResultModel<Gym>(null, 404);

                _ctx.Entry(getResponse).State = EntityState.Detached;

                //update gym
                _ctx.Entry(getResponse).CurrentValues.SetValues(gym);

                //Delete children
                foreach (var existingTraining in getResponse.Trainings.ToList())
                {
                    if (!gym.Trainings.Any(t => t.TrainingId == existingTraining.TrainingId))
                    {
                        _ctx.Trainings.Remove(existingTraining);
                    }
                }

                //Add and update children
                foreach (var trainingModel in gym.Trainings)
                {
                    var existingTraining = getResponse.Trainings
                        .Where(t => t.TrainingId == trainingModel.TrainingId)
                        .SingleOrDefault();

                    if (existingTraining != null)
                    {
                        _ctx.Entry(existingTraining).CurrentValues.SetValues(trainingModel);
                    }
                    else
                    {
                        var newTraining = new Training
                        {
                            Description = trainingModel.Description,
                            DateTime = trainingModel.DateTime,
                            TrainingUsers = trainingModel.TrainingUsers,
                            Gym = trainingModel.Gym,
                            TrainingId = trainingModel.TrainingId
                        };
                    }
                }             
                
                _ctx.SaveChanges();

                return new ResultModel<Gym>(gym, 200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ResultModel<Gym>(null, 500);
            }
        }
        public void Dispose()
        {
            _ctx?.Dispose();
        }
    }
}