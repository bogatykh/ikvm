package sun.nio.ch;

import java.io.*;
import java.nio.*;
import java.nio.channels.*;
import java.util.concurrent.*;

/**
 * .NET implementation of AsynchronousFileChannel.
 */
public class DotNetAsynchronousFileChannelImpl extends AsynchronousFileChannelImpl implements Cancellable, Groupable {

    private static class DefaultGroupHolder {

        static final DotNetAsynchronousChannelGroup defaultGroup = defaultGroup();

        private static DotNetAsynchronousChannelGroup defaultGroup() {
            try {
                return new DotNetAsynchronousChannelGroup(null, ThreadPool.createDefault());
            } catch (IOException ioe) {
                throw new InternalError(ioe);
            }
        }

    }

    public static AsynchronousFileChannel open(FileDescriptor fdo, boolean reading, boolean writing, ThreadPool pool) throws IOException
    {
        DotNetAsynchronousChannelGroup group;
        boolean isDefaultGroup;

        if (pool == null) {
            group = DefaultGroupHolder.defaultGroup;
            isDefaultGroup = true;
        } else {
            group = new DotNetAsynchronousChannelGroup(null, pool);
            isDefaultGroup = false;
        }
        try {
            return new DotNetAsynchronousFileChannelImpl(fdo, reading, writing, group, isDefaultGroup);
        } catch (IOException x) {
            // error binding to port so need to close it (if created for this channel)
            if (!isDefaultGroup)
                group.implClose();

            throw x;
        }
    }

    protected final DotNetAsynchronousChannelGroup group;
    protected final boolean isDefaultGroup;

    private DotNetAsynchronousFileChannelImpl(FileDescriptor fdObj, boolean reading, boolean writing, DotNetAsynchronousChannelGroup group, boolean isDefaultGroup) throws IOException {
        super(fdObj, reading, writing, group.executor());
        this.group = group;
        this.isDefaultGroup = isDefaultGroup;
    }

    @Override
    public AsynchronousChannelGroupImpl group() {
        return group;
    }

    @Override
    public void onCancel(PendingFuture<?, ?> task) {
        onCancel0(task);
    }

    @Override
    public void close() throws IOException {
        close0();
    }

    @Override
    public long size() throws IOException {
        return size0();
    }

    @Override
    public AsynchronousFileChannel truncate(long size) throws IOException {
        return truncate0(size);
    }

    @Override
    public void force(boolean metaData) throws IOException {
        force0(metaData);
    }

    @Override
    <A> Future<FileLock> implLock(final long position, final long size, final boolean shared, A attachment, final CompletionHandler<FileLock, ? super A> handler) {
        return implLock0(position, size, shared, attachment, handler);
    }

    @Override
    public FileLock tryLock(long position, long size, boolean shared) throws IOException {
        return tryLock0(position, size, shared);
    }

    @Override
    protected void implRelease(FileLockImpl fli) throws IOException {
        implRelease0(fli);
    }

    @Override
    <A> Future<Integer> implRead(ByteBuffer dst, long position, A attachment, CompletionHandler<Integer, ? super A> handler)
    {
        return implRead0(dst, position, attachment, handler);
    }

    <A> Future<Integer> implWrite(ByteBuffer src, long position, A attachment, CompletionHandler<Integer, ? super A> handler) {
        return implWrite0(src, position, attachment, handler);
    }

    private native void onCancel0(PendingFuture<?, ?> task);

    private native void close0();

    private native long size0();

    private native AsynchronousFileChannel truncate0(long size);

    private native void force0(boolean metaData);

    private native <A> Future<FileLock> implLock0(final long position, final long size, final boolean shared, A attachment, final CompletionHandler<FileLock, ? super A> handler);
            
    private native FileLock tryLock0(long position, long size, boolean shared);
            
    private native void implRelease0(FileLockImpl fli);
            
    private native <A> Future<Integer> implRead0(ByteBuffer dst, long position, A attachment, CompletionHandler<Integer, ? super A> handler);
            
    private native <A> Future<Integer> implWrite0(ByteBuffer src, long position, A attachment, CompletionHandler<Integer, ? super A> handler);

}
