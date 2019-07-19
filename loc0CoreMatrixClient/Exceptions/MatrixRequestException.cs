using System;

namespace loc0CoreMatrixClient.Exceptions
{
    /// <summary>
    /// General exception for errors relating to the client
    /// </summary>
    public class MatrixRequestException : Exception
    {
        /// <inheritdoc />
        public MatrixRequestException()
        {
        }

        /// <inheritdoc />
        /// <param name="message">Message to be displayed</param>
        public MatrixRequestException(string message) :
            base(message)
        {
        }

        /// <inheritdoc />
        /// <param name="message">Message to be displayed</param>
        /// <param name="innerException"></param>
        public MatrixRequestException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}