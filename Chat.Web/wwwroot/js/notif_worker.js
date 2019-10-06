self.addEventListener('push', event => {
    const title = 'Groups Notification';
    const options = {
        body: "You have been invited to " + event.data.text(),
        icon: '/images/icon192.png',
        badge: '/images/badge72.png'
    };
    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    //todo finish worker here and up there
});