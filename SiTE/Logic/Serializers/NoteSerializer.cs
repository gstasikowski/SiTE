using CustomDatabase.Helpers;
using SiTE.Models;
using System;

namespace SiTE.Logic.Serializers
{
    class NoteSerializer
    {
        public byte[] Serialize(NoteModel note)
        {
            byte[] titleBytes = System.Text.Encoding.UTF8.GetBytes(note.Title);
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(note.Content);
            byte[] createDateBytes = System.Text.Encoding.UTF8.GetBytes(note.Created.ToString());
            byte[] modifiedDateBytes = System.Text.Encoding.UTF8.GetBytes(note.Modified.ToString());
            byte[] noteData = new byte[
                16 +                   // 16 bytes for Guid ID
                4 +                    // 4 bytes indicate the length of title string
                titleBytes.Length +    // n bytes for breed string
                4 +                    // 4 bytes indicate the length of the content string
                contentBytes.Length +  // z bytes for content
                4 +                      // 4 bytes indicate the length of the creation date
                createDateBytes.Length + // x bytes for creation date
                4 +                      // 4 bytes indicate length of modified date
                modifiedDateBytes.Length      // y bytes of modified date
                ];

            // ID
            Buffer.BlockCopy(
                src: note.ID.ToByteArray(),
                srcOffset: 0,
                dst: noteData,
                dstOffset: 0,
                count: 16
                );

            // Title
            Buffer.BlockCopy(
                src: LittleEndianByteOrder.GetBytes(titleBytes.Length),
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16,
                count: 4
                );

            Buffer.BlockCopy(
                src: titleBytes,
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4,
                count: titleBytes.Length
                );

            // Content
            Buffer.BlockCopy(
                src: LittleEndianByteOrder.GetBytes(contentBytes.Length),
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length,
                count: 4
                );

            Buffer.BlockCopy(
                src: contentBytes,
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length + 4,
                count: contentBytes.Length
                );

            // Create date
            Buffer.BlockCopy(
                src: LittleEndianByteOrder.GetBytes(createDateBytes.Length),
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length + 4 + contentBytes.Length,
                count: 4
                );

            Buffer.BlockCopy(
                src: createDateBytes,
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length + 4 + contentBytes.Length + 4,
                count: createDateBytes.Length
                );

            // Modify date
            Buffer.BlockCopy(
                src: LittleEndianByteOrder.GetBytes(modifiedDateBytes.Length),
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length + 4 + contentBytes.Length + 4 + createDateBytes.Length,
                count: 4
                );

            Buffer.BlockCopy(
                src: modifiedDateBytes,
                srcOffset: 0,
                dst: noteData,
                dstOffset: 16 + 4 + titleBytes.Length + 4 + contentBytes.Length + 4 + createDateBytes.Length + 4,
                count: modifiedDateBytes.Length
                );

            return noteData;
        }

        public NoteModel Deserialize(byte[] data)
        {
            var note = new NoteModel();

            // ID
            note.ID = BufferHelper.ReadBufferGuid(data, 0);

            // Title
            int titleLength = BufferHelper.ReadBufferInt32(data, 16);

            if (titleLength < 0 || titleLength > (16 * 1024))
            { throw new Exception("Invalid string length: " + titleLength); }

            note.Title = System.Text.Encoding.UTF8.GetString(data, 16 + 4, titleLength);

            // Content
            int contentLength = BufferHelper.ReadBufferInt32(data, 16 + 4 + titleLength);

            if (contentLength < 0 || contentLength > (16 * 1024))
            { throw new Exception("Invalid string length: " + contentLength); }

            note.Content = System.Text.Encoding.UTF8.GetString(data, 16 + 4 + titleLength + 4, contentLength);

            // Create date
            int createDateLength = BufferHelper.ReadBufferInt32(data, 16 + 4 + titleLength + 4 + contentLength);

            if (createDateLength < 0 || createDateLength > (16 * 1024))
            { throw new Exception("Invalid string length: " + createDateLength); }

            DateTime createDate = DateTime.Parse(System.Text.Encoding.UTF8.GetString(data, 16 + 4 + titleLength + 4 + contentLength + 4, createDateLength));
            note.Created = createDate;

            // Modify date
            int modifyDateLength = BufferHelper.ReadBufferInt32(data, 16 + 4 + titleLength + 4 + contentLength + 4 + createDateLength);

            if (modifyDateLength < 0 || modifyDateLength > (16 * 1024))
            { throw new Exception("Invalid string length: " + modifyDateLength); }

            DateTime modifyDate = DateTime.Parse(System.Text.Encoding.UTF8.GetString(data, 16 + 4 + titleLength + 4 + contentLength + 4 + createDateLength + 4, modifyDateLength));
            note.Modified = modifyDate;

            return note;
        }
        
        public NoteModel DeserializeSimple(byte[] data)
        {
            var note = new NoteModel();

            // ID
            note.ID = BufferHelper.ReadBufferGuid(data, 0);

            // Title
            int titleLength = BufferHelper.ReadBufferInt32(data, 16);

            if (titleLength < 0 || titleLength > (16 * 1024))
            { throw new Exception("Invalid string length: " + titleLength); }

            note.Title = System.Text.Encoding.UTF8.GetString(data, 16 + 4, titleLength);

            return note;
        }
    }
}
