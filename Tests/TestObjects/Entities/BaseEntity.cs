﻿using Emberpoint.Core.GameObjects.Interfaces;
using Emberpoint.Core.GameObjects.Managers;
using Emberpoint.Core.GameObjects.Map;
using GoRogue;
using Microsoft.Xna.Framework;
using System;
using Tests.TestObjects.Grids;
using static SadConsole.Entities.Entity;

namespace Tests.TestObjects.Entities
{
    public class BaseEntity : IEntity
    {
        private Point _position;
        public Point Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value.X != _position.X || value.Y != _position.Y)
                {
                    Moved?.Invoke(this, new SadConsole.Entities.Entity.EntityMovedEventArgs(null, _position));
                }
                _position = value;
            }
        }

        public Direction Facing { get; private set; }

        public EventHandler<EntityMovedEventArgs> Moved;

        public int FieldOfViewRadius { get; set; } = 0;

        private FOV _fieldOfView;
        public FOV FieldOfView
        {
            get => _fieldOfView ??= new FOV(_grid.FieldOfView);
        }

        public int ObjectId { get; }
        public int Health { get; private set; }

        private int _maxHealth;
        public int MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                Health = _maxHealth;
            }
        }

        public int Glyph => throw new NotImplementedException();

        public SadConsole.Console RenderConsole => throw new NotImplementedException("Unit tests do not use XNA consoles.");

        private readonly EmberGrid _grid;

        public BaseEntity()
        {
            // Not linked to a grid
            ObjectId = EntityManager.GetUniqueId();
            Moved += OnMove;
            MaxHealth = 100; // Default stats
            Facing = Direction.DOWN;
        }

        public BaseEntity(BaseGrid grid)
        {
            _grid = grid;
            ObjectId = EntityManager.GetUniqueId();
            Moved += OnMove;
            MaxHealth = 100; // Default stats
            Facing = Direction.DOWN;
        }

        private void OnMove(object sender, EntityMovedEventArgs args)
        {
            if (_grid != null)
            {
                if (FieldOfViewRadius > -1)
                {
                    // Re-calculate the field of view
                    FieldOfView.Calculate(Position, FieldOfViewRadius);
                }
            }
        }

        private void ExecuteMovementEffects(EntityMovedEventArgs args)
        {
            // Check if we moved
            if (args.FromPosition != Position)
            {
                var cell = _grid.GetCell(Position);
                if (cell.EffectProperties.EntityMovementEffects != null)
                {
                    foreach (var effect in cell.EffectProperties.EntityMovementEffects)
                    {
                        effect(this);
                    }
                }
            }
        }

        public bool CanMoveTowards(Point position)
        {
            if (Health == 0) return false;
            return _grid.InBounds(position) && _grid.GetCell(position).CellProperties.Walkable && !EntityManager.EntityExistsAt(position);
        }

        public void MoveTowards(Point position, bool checkCanMove = true, Direction direction = null, bool triggerMovementEffects = true)
        {
            if (Health == 0) return;

            // Set correct facing direction regardless if we can move or not
            SetFacingDirection(position, direction);

            if (checkCanMove && !CanMoveTowards(position)) return;
            var prevPos = Position;

            Position = position;

            if (prevPos != position)
            {
                var args = new EntityMovedEventArgs(null, prevPos);
                Moved.Invoke(this, args);

                if (triggerMovementEffects)
                {
                    // Check if the cell has movement effects to be executed
                    ExecuteMovementEffects(args);
                }
            }
        }

        public void MoveTowards(Direction position, bool checkCanMove = true)
        {
            var pos = Position;
            MoveTowards(pos += position, checkCanMove);
        }

        private void SetFacingDirection(Point newPosition, Direction direction)
        {
            // Set facing direction
            var prevPos = Position;
            var difference = newPosition - prevPos;
            if (difference.X == 1 && difference.Y == 0)
                Facing = Direction.RIGHT;
            else if (difference.X == -1 && difference.Y == 0)
                Facing = Direction.LEFT;
            else if (difference.X == 0 && difference.Y == 1)
                Facing = Direction.DOWN;
            else if (difference.X == 0 && difference.Y == -1)
                Facing = Direction.UP;
            else
                Facing = direction ?? Direction.DOWN;
        }

        public void ResetFieldOfView()
        {
            _fieldOfView = null;
        }

        public bool GetInteractedCell(out Point cellPosition)
        {
            cellPosition = default;
            var facingPosition = Position + Facing;
            if (CanInteract(facingPosition.X, facingPosition.Y))
            {
                cellPosition = new Point(facingPosition.X, facingPosition.Y);
                return true;
            }
            return false;
        }

        public bool CheckInteraction()
        {
            if (GetInteractedCell(out _))
            {
                return true;
            }
            return false;
        }

        public bool CanInteract(int x, int y)
        {
            if (Health == 0) return false;
            if (!_grid.InBounds(x, y)) return false;
            var cell = _grid.GetCell(x, y);
            return cell.CellProperties.Interactable && cell.CellProperties.IsExplored;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;

                // Handle entity death
                // UnRenderObject();
                EntityManager.Remove(this);
            }
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }

        public void RenderObject(SadConsole.Console console)
        {
            throw new NotImplementedException();
        }

        public void UnRenderObject()
        {
            throw new NotImplementedException();
        }
    }
}
