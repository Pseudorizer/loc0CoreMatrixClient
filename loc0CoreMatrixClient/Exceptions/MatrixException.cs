using System;

namespace loc0CoreMatrixClient.Exceptions
{
    /// <summary>
    /// General exception for errors relating to the client
    /// </summary>
    public class MatrixException : Exception
    {
        /// <inheritdoc />
        public MatrixException()
        {
        }

        /// <inheritdoc />
        /// <param name="message">Message to be displayed</param>
        public MatrixException(string message) :
            base(message)
        {
        }

        /// <inheritdoc />
        /// <param name="message">Message to be displayed</param>
        /// <param name="innerException"></param>
        public MatrixException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}