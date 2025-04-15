using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WolfClone.FirstPerson.Engine;

internal class Player
{
    public Vector3 Position { get; private set; }
    public Vector3 Direction { get; private set; }

    public Player(float posX, float posY, float dirX = 1, float dirY = 0)
    {
        Position = new Vector3(posX, posY, 0.5f);
        Direction = new Vector3(dirX, dirY, 0);
        Direction = Vector3.Normalize(Direction);
    }

    public void DebugMovement(float deltaTime, Vector3 input)
    {
        float moveSpeed = 5.0f;
        float rotationSpeed = 3.0f;

        float moveStep = -input.Y * moveSpeed * deltaTime;
        Position += new Vector3(moveStep * Direction.X, moveStep * Direction.Y, 0);

        float rotAngle = input.X * rotationSpeed * deltaTime;
        Matrix rotationMatrix = Matrix.CreateRotationZ(rotAngle);
        Direction = Vector3.Transform(Direction, rotationMatrix);
        Direction = Vector3.Normalize(Direction);
    }
}
