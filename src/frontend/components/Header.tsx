import Link from 'next/link';
import { useEffect, useState } from 'react';
import { Role, User } from '../types/index';
import { useRouter } from 'next/router';

const Header: React.FC = () => {
    const router = useRouter();
    const [loggedInUser, setLoggedInUser] = useState<User | null>(null);

    useEffect(() => {
        const user = localStorage.getItem('loggedInUser');
        if (user) setLoggedInUser(JSON.parse(user));
    }, []);

    const handleLogout = () => {
        localStorage.removeItem('loggedInUser');
        setLoggedInUser(null);
        router.replace('/');
    };

    return (
        <header className="p-4 border-bottom bg-gradient-to-br from-gray-900 to-gray-600 flex items-center justify-between">
            <div className="flex items-center gap-2">
                <h1 className="text-white text-4xl">Frontend</h1>
            </div>
            <nav className="flex items-center">
                {!loggedInUser && (
                    <Link
                        href="/auth"
                        className="px-4 py-2 text-white text-xl hover:bg-gray-600 rounded-lg"
                    >
                        Login
                    </Link>
                )}
                {loggedInUser && (
                    <>
                        <Link
                            href="/"
                            className="px-4 pt-1 pb-2 text-white text-xl text-white hover:bg-gray-600 rounded-lg"
                        >
                            Home
                        </Link>
                        {loggedInUser.role === Role.ADMIN ? (
                            <Link
                                href="/admin"
                                className="px-4 pt-1 pb-2 text-white text-xl text-white hover:bg-gray-600 rounded-lg"
                            >
                                Admin Panel
                            </Link>
                        ) : null}
                        <Link
                            href="/"
                            onClick={handleLogout}
                            className="px-4 pt-1 pb-2 text-white text-xl text-white hover:bg-gray-600 rounded-lg"
                        >
                            Logout
                        </Link>
                        <Link
                            href="/account"
                            className="px-4 pt-1 pb-2 text-white text-xl text-white hover:bg-gray-600 border-l"
                        >
                            {loggedInUser.username}
                        </Link>{' '}
                    </>
                )}
            </nav>
        </header>
    );
};

export default Header;
